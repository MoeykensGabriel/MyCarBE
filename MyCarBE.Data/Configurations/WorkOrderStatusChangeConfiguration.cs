using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class WorkOrderStatusChangeConfiguration : IEntityTypeConfiguration<WorkOrderStatusChange>
{
    public void Configure(EntityTypeBuilder<WorkOrderStatusChange> builder)
    {
        builder.ToTable("WorkOrderStatusChanges");
        builder.HasKey(s => s.Id);

        // Evento inmutable: todos los campos son de solo escritura después del insert
        builder.Property(s => s.ChangedAt).IsRequired();
        builder.Property(s => s.Note).HasMaxLength(1000);

        // No hay soft delete ni query filter — los eventos no se borran nunca
        builder.HasOne(s => s.WorkOrder)
               .WithMany(w => w.StatusChanges)
               .HasForeignKey(s => s.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
