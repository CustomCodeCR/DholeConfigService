using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Config.Domain.Catalogs.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Config.Persistence.Configurations.Catalogs;

internal sealed class CatalogGroupConfiguration : EntityTypeConfigurationBase<CatalogGroup, Guid>
{
    public override void Configure(EntityTypeBuilder<CatalogGroup> builder)
    {
        base.Configure(builder);

        builder.ToTable("CatalogGroups");

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");

        builder.Property(x => x.IsSystem).IsRequired().HasDefaultValue(false);

        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        builder
            .HasMany(x => x.Items)
            .WithOne(x => x.CatalogGroup)
            .HasForeignKey(x => x.CatalogGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
