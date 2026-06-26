using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.SellerName).IsRequired().HasMaxLength(200);

        // A quién: cliente XOR flota. Restrict para no borrar ventas si se borra el comprador.
        builder.HasOne(s => s.Customer)
               .WithMany()
               .HasForeignKey(s => s.CustomerId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasOne(s => s.Fleet)
               .WithMany()
               .HasForeignKey(s => s.FleetId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasMany(s => s.Items)
               .WithOne(i => i.Sale)
               .HasForeignKey(i => i.SaleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.FleetId);
        builder.HasIndex(s => s.SellerUserId);
        builder.HasIndex(s => s.CreatedAt);
    }
}
