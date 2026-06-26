using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductCode).HasMaxLength(100);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");

        // Calculado — no se persiste. La relación con Sale la define SaleConfiguration.
        builder.Ignore(i => i.Subtotal);

        builder.HasIndex(i => i.SaleId);
    }
}
