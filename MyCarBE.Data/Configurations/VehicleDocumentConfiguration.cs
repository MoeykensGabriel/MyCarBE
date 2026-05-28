using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleDocumentConfiguration : IEntityTypeConfiguration<VehicleDocument>
{
    public void Configure(EntityTypeBuilder<VehicleDocument> builder)
    {
        builder.ToTable("VehicleDocuments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentType).HasConversion<int>();
        builder.Property(d => d.Notes).HasMaxLength(1000);
        builder.Property(d => d.IssuingEntity).HasMaxLength(200);

        builder.HasOne(d => d.Vehicle)
               .WithMany(v => v.Documents)
               .HasForeignKey(d => d.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.VehicleId);
        builder.HasIndex(d => d.ExpiresOn);
        // Para query de vencimientos próximos por cliente, el filtro arranca en Vehicles
        // así que no hace falta un índice compuesto (VehicleId, ExpiresOn).

        // El query filter del soft-delete se aplica automáticamente por BaseEntity en AppDbContext.
    }
}
