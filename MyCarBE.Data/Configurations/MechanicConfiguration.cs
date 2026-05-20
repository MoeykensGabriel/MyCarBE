using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class MechanicConfiguration : IEntityTypeConfiguration<Mechanic>
{
    public void Configure(EntityTypeBuilder<Mechanic> builder)
    {
        builder.ToTable("Mechanics");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.LastName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Phone).HasMaxLength(30);
        builder.Property(m => m.Specialty).HasMaxLength(200);
        builder.Property(m => m.IsActive).HasDefaultValue(true);

        builder.HasIndex(m => m.Email).IsUnique();
        builder.HasIndex(m => m.ApplicationUserId).IsUnique();
        builder.HasIndex(m => m.IsActive);
    }
}
