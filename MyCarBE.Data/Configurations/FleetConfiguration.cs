using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class FleetConfiguration : IEntityTypeConfiguration<Fleet>
{
    public void Configure(EntityTypeBuilder<Fleet> builder)
    {
        builder.ToTable("Fleets");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(f => f.TaxId).IsRequired().HasMaxLength(20);   // CUIT
        builder.Property(f => f.Phone).IsRequired().HasMaxLength(30);
        builder.Property(f => f.Email).HasMaxLength(150);
        builder.Property(f => f.Address).HasMaxLength(300);

        builder.HasIndex(f => f.TaxId).IsUnique();
    }
}
