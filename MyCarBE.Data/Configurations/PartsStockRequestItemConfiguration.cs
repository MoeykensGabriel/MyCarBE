using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Configurations;

public class PartsStockRequestItemConfiguration : IEntityTypeConfiguration<PartsStockRequestItem>
{
    public void Configure(EntityTypeBuilder<PartsStockRequestItem> builder)
    {
        builder.ToTable("PartsStockRequestItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductCode)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(i => i.Status)
               .HasConversion<int>()
               .HasDefaultValue(StockRequestItemStatus.PendingReview);

        builder.Property(i => i.Notes)
               .HasMaxLength(500);

        builder.HasOne(i => i.PartsStockRequest)
               .WithMany(r => r.Items)
               .HasForeignKey(i => i.PartsStockRequestId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.PartsStockRequestId);
        builder.HasIndex(i => i.WorkOrderPartId);
        builder.HasIndex(i => i.ProductCode);
    }
}
