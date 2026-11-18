using System.Text.Json;
using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Dhole.Config.Domain.Catalogs.Entities;
using Dhole.Config.Persistence.Auditing;
using Dhole.Config.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dhole.Config.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options,
    IConfiguration? realtimeConfiguration = null)
    : AppDbContextBase(options)
{
    private const string SourceService = "DholeConfigService";

    public DbSet<CatalogGroup> CatalogGroups => Set<CatalogGroup>();
    public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
    public DbSet<SupplierBankRecord> SupplierBankRecords => Set<SupplierBankRecord>();
    public DbSet<SupplierFollowupRecord> SupplierFollowups => Set<SupplierFollowupRecord>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddRealtimeInvalidations();
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AddRealtimeInvalidations();
        AddDomainEventsToOutbox();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("config");

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);

        modelBuilder.Entity<SupplierBankRecord>(entity =>
        {
            entity.ToTable("srm_supplier_bank_records", "config");
            entity.HasKey(x => x.SupplierId).HasName("pk_srm_supplier_bank_records");
            entity.Property(x => x.SupplierId).HasColumnName("supplier_id");
            entity.Property(x => x.Ciphertext).IsRequired().HasColumnName("ciphertext");
            entity.Property(x => x.Nonce).IsRequired().HasColumnName("nonce");
            entity.Property(x => x.Tag).IsRequired().HasColumnName("tag");
            entity.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");
        });

        modelBuilder.Entity<SupplierFollowupRecord>(entity =>
        {
            entity.ToTable("srm_supplier_followups", "config");
            entity.HasKey(x => x.Id).HasName("pk_srm_supplier_followups");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SupplierId).HasColumnName("supplier_id");
            entity.Property(x => x.Kind).HasMaxLength(30).IsRequired().HasColumnName("kind");
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired().HasColumnName("title");
            entity.Property(x => x.Details).HasMaxLength(4000).IsRequired().HasColumnName("details");
            entity.Property(x => x.NextActionAtUtc).HasColumnName("next_action_at_utc");
            entity.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
            entity.Property(x => x.CreatedById).HasMaxLength(160).IsRequired().HasColumnName("created_by_id");
            entity.Property(x => x.CreatedByName).HasMaxLength(200).IsRequired().HasColumnName("created_by_name");
            entity.HasIndex(x => new { x.SupplierId, x.CreatedAtUtc })
                .HasDatabaseName("ix_srm_supplier_followups_supplier_created");
        });

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }

    private void AddRealtimeInvalidations()
    {
        if (!ConfigRealtimeInvalidation.IsEnabled(realtimeConfiguration))
            return;
        ChangeTracker.DetectChanges();
        var supplierGroupId = ChangeTracker.Entries<CatalogGroup>()
            .Select(x => x.Entity)
            .FirstOrDefault(x => x.Slug == "srm-suppliers")?.Id ?? Guid.Empty;
        if (supplierGroupId == Guid.Empty && ChangeTracker.Entries<CatalogItem>()
            .Any(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            supplierGroupId = CatalogGroups.AsNoTracking()
                .Where(x => x.Slug == "srm-suppliers")
                .Select(x => x.Id).FirstOrDefault();
        var catalogListChanged = false;
        foreach (var entry in ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToArray())
        {
            string kind;
            Guid aggregateId;
            switch (entry.Entity)
            {
                case CatalogGroup group:
                    kind = "catalog-group";
                    aggregateId = group.Id;
                    break;
                case CatalogItem item:
                    kind = supplierGroupId != Guid.Empty && item.CatalogGroupId == supplierGroupId
                        ? "srm-supplier" : "catalog-item";
                    aggregateId = item.Id;
                    break;
                default:
                    continue;
            }
            if (aggregateId == Guid.Empty) continue;
            catalogListChanged = true;
            var eventId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var correlationId =
                (AuditExecutionContextAccessor.Current?.CorrelationId ?? eventId).ToString("D");
            var eventType = ConfigRealtimeInvalidation.EventType(kind);
            OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventName = eventType,
                EventType = eventType,
                SourceService = SourceService,
                PayloadJson = ConfigRealtimeInvalidation.Serialize(
                    eventId, aggregateId, kind,
                    ConfigRealtimeInvalidation.NextVersionHint(), now, correlationId),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = now.UtcDateTime
            });
        }

        // A newly created item has no known item ID on other clients.
        // Send one sanitized list invalidation from the same DB transaction.
        if (catalogListChanged)
        {
            var eventId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var correlationId =
                (AuditExecutionContextAccessor.Current?.CorrelationId ?? eventId).ToString("D");
            var eventType = ConfigRealtimeInvalidation.EventType("catalog-list");
            OutboxMessages.Add(new OutboxMessage
            {
                EventId = eventId,
                EventName = eventType,
                EventType = eventType,
                SourceService = SourceService,
                PayloadJson = ConfigRealtimeInvalidation.Serialize(eventId,
                    ConfigRealtimeInvalidation.CatalogListId, "catalog-list",
                    ConfigRealtimeInvalidation.NextVersionHint(), now, correlationId),
                CorrelationId = correlationId,
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                CreatedAtUtc = now.UtcDateTime
            });
        }
    }

    private void AddDomainEventsToOutbox()
    {
        var aggregateRoots = ChangeTracker
            .Entries()
            .Select(x => x.Entity)
            .OfType<AggregateRoot<Guid>>()
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();

        if (aggregateRoots.Count == 0)
        {
            return;
        }

        var outboxMessages = new List<OutboxMessage>();

        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var domainEvent in aggregateRoot.DomainEvents)
            {
                var eventType = DomainEventOutboxMapper.GetEventType(domainEvent);
                var eventName = DomainEventOutboxMapper.GetEventName(domainEvent);
                var payloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

                var correlationId =
                    AuditExecutionContextAccessor.Current?.CorrelationId ?? Guid.NewGuid();

                outboxMessages.Add(
                    new OutboxMessage
                    {
                        EventId = domainEvent.EventId,
                        EventType = eventType,
                        EventName = eventName,
                        SourceService = SourceService,
                        PayloadJson = payloadJson,
                        HeadersJson = null,
                        CorrelationId = correlationId.ToString(),
                        Status = OutboxMessageStatus.Pending,
                        RetryCount = 0,
                        ErrorMessage = null,
                        CreatedAtUtc = DateTime.UtcNow,
                    }
                );
            }

            aggregateRoot.ClearDomainEvents();
        }

        OutboxMessages.AddRange(outboxMessages);
    }
}
