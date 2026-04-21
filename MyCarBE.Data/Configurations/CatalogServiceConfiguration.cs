using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class CatalogServiceConfiguration : IEntityTypeConfiguration<CatalogService>
{
    public void Configure(EntityTypeBuilder<CatalogService> builder)
    {
        builder.ToTable("CatalogServices");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.DefaultPrice).HasColumnType("numeric(18,2)");

        builder.HasIndex(c => c.Name).IsUnique();
    }
}
