using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.DocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(150);

        builder.HasIndex(c => c.DocumentNumber).IsUnique();
        builder.HasIndex(c => c.Phone).IsUnique();

        builder.HasOne(c => c.Fleet)
               .WithMany(f => f.Contacts)
               .HasForeignKey(c => c.FleetId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
