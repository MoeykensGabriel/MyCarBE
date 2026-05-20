using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class WorkOrderPhotoConfiguration : IEntityTypeConfiguration<WorkOrderPhoto>
{
    public void Configure(EntityTypeBuilder<WorkOrderPhoto> builder)
    {
        builder.ToTable("WorkOrderPhotos");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Url).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Caption).HasMaxLength(300);

        builder.HasOne(p => p.WorkOrder)
               .WithMany(w => w.Photos)
               .HasForeignKey(p => p.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // Nullable: null = foto general, con valor = foto de un servicio específico
        builder.HasOne(p => p.WorkOrderService)
               .WithMany(s => s.Photos)
               .HasForeignKey(p => p.WorkOrderServiceId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
