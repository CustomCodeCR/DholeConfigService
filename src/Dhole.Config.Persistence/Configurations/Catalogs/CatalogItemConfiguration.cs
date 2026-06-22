using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Config.Domain.Catalogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Config.Persistence.Configurations.Catalogs;

internal sealed class CatalogItemConfiguration : EntityTypeConfigurationBase<CatalogItem, Guid>
{
    public override void Configure(EntityTypeBuilder<CatalogItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("CatalogItems");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CatalogGroupId).IsRequired();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();

        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();

        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.Value).HasMaxLength(500);

        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");

        builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);

        builder.Property(x => x.IsSystem).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => new { x.CatalogGroupId, x.Code }).IsUnique();

        builder.HasIndex(x => new { x.CatalogGroupId, x.Slug }).IsUnique();

        builder.HasIndex(x => new { x.CatalogGroupId, x.Name }).IsUnique();

        builder.HasIndex(x => new { x.CatalogGroupId, x.SortOrder });

        builder
            .HasOne(x => x.CatalogGroup)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CatalogGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
