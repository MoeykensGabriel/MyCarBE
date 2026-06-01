using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles", t => t.HasCheckConstraint(
            "CK_Vehicles_Owner_XOR",
            "(\"CustomerId\" IS NOT NULL AND \"FleetId\" IS NULL) OR (\"CustomerId\" IS NULL AND \"FleetId\" IS NOT NULL)"
        ));

        builder.HasKey(v => v.Id);

        builder.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);
        builder.Property(v => v.Brand).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Model).IsRequired().HasMaxLength(100);
        builder.Property(v => v.VIN).HasMaxLength(17);
        builder.Property(v => v.EngineNumber).HasMaxLength(50);
        builder.Property(v => v.Color).HasMaxLength(50);
        builder.Property(v => v.RegistrationHolderFirstName).IsRequired().HasMaxLength(100);
        builder.Property(v => v.RegistrationHolderLastName).IsRequired().HasMaxLength(100);
        builder.Property(v => v.RegistrationHolderDocumentNumber).IsRequired().HasMaxLength(50);
        builder.Property(v => v.RegistrationCertificateNumber).HasMaxLength(50);

        builder.HasIndex(v => v.LicensePlate).IsUnique();
        builder.HasIndex(v => v.VIN).IsUnique().HasFilter("\"VIN\" IS NOT NULL");

        // Nota: los índices sobre CustomerId y FleetId ya los crea EF Core automáticamente
        // por ser foreign keys (ver HasOne/WithMany abajo). Aceleran el filtro por dueño del
        // listado paginado (SearchPagedAsync) sin necesidad de declararlos explícitamente.

        // Token público para la estación de viajes (QR del chofer). Único y filtrado
        // para permitir múltiples vehículos sin token todavía.
        builder.Property(v => v.TripToken).HasMaxLength(64);
        builder.HasIndex(v => v.TripToken).IsUnique().HasFilter("\"TripToken\" IS NOT NULL");

        builder.HasOne(v => v.Customer)
               .WithMany(c => c.Vehicles)
               .HasForeignKey(v => v.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Fleet)
               .WithMany(f => f.Vehicles)
               .HasForeignKey(v => v.FleetId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
