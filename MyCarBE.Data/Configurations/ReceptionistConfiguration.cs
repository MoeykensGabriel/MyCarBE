using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.ToTable("Receptionists");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.LastName).IsRequired().HasMaxLength(100);
        builder.Property(r => r.Email).IsRequired().HasMaxLength(150);
        builder.Property(r => r.IsActive).HasDefaultValue(true);

        builder.HasIndex(r => r.Email).IsUnique();
        builder.HasIndex(r => r.ApplicationUserId).IsUnique();
        builder.HasIndex(r => r.IsActive);
    }
}
