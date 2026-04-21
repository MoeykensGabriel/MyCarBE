using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class DeclaredServiceHistoryConfiguration : IEntityTypeConfiguration<DeclaredServiceHistory>
{
    public void Configure(EntityTypeBuilder<DeclaredServiceHistory> builder)
    {
        builder.ToTable("DeclaredServiceHistories");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description).IsRequired().HasMaxLength(1000);
        builder.Property(d => d.Workshop).HasMaxLength(200);
        builder.Property(d => d.Notes).HasMaxLength(1000);

        builder.HasOne(d => d.Vehicle)
               .WithMany(v => v.DeclaredServiceHistories)
               .HasForeignKey(d => d.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
