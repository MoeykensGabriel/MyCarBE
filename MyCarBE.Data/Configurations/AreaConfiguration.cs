using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area>
{
    // Guids estables para el seed — no cambiar (compatibilidad de migraciones)
    public static readonly Guid MotorId            = Guid.Parse("11111111-0000-0000-0000-000000000001");
    public static readonly Guid FrenosId           = Guid.Parse("11111111-0000-0000-0000-000000000002");
    public static readonly Guid TrenDelanteroId    = Guid.Parse("11111111-0000-0000-0000-000000000003");
    public static readonly Guid SuspensionId       = Guid.Parse("11111111-0000-0000-0000-000000000004");
    public static readonly Guid ElectricoId        = Guid.Parse("11111111-0000-0000-0000-000000000005");
    public static readonly Guid TransmisionId      = Guid.Parse("11111111-0000-0000-0000-000000000006");
    public static readonly Guid EscapeId           = Guid.Parse("11111111-0000-0000-0000-000000000007");
    public static readonly Guid CarroceriaId       = Guid.Parse("11111111-0000-0000-0000-000000000008");
    public static readonly Guid AireAcondicionadoId = Guid.Parse("11111111-0000-0000-0000-000000000009");
    public static readonly Guid DiagnosticoCompId  = Guid.Parse("11111111-0000-0000-0000-00000000000a");
    public static readonly Guid CubiertasId        = Guid.Parse("11111111-0000-0000-0000-00000000000b");
    public static readonly Guid BateriaId          = Guid.Parse("11111111-0000-0000-0000-00000000000c");

    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
        builder.Property(a => a.IsActive).HasDefaultValue(true);
        builder.Property(a => a.IsTireArea).HasDefaultValue(false);
        builder.Property(a => a.IsBatteryArea).HasDefaultValue(false);

        builder.HasIndex(a => a.Name).IsUnique();
        builder.HasIndex(a => a.IsActive);

        // Relación M-a-N — EF Core 9 crea tabla puente "MechanicArea" automáticamente
        builder.HasMany(a => a.Mechanics)
               .WithMany(m => m.Areas)
               .UsingEntity(j => j.ToTable("MechanicAreas"));

        // Seed de 10 áreas iniciales (CreatedAt/UpdatedAt fijos para evitar drift de migraciones)
        var seedDate = new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new Area { Id = MotorId,             Name = "Motor",                  IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = FrenosId,            Name = "Frenos",                 IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = TrenDelanteroId,     Name = "Tren delantero",         IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = SuspensionId,        Name = "Suspensión",             IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = ElectricoId,         Name = "Eléctrico",              IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = TransmisionId,       Name = "Transmisión",            IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = EscapeId,            Name = "Escape",                 IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = CarroceriaId,        Name = "Carrocería",             IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = AireAcondicionadoId, Name = "Aire acondicionado",     IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = DiagnosticoCompId,   Name = "Diagnóstico computarizado", IsActive = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = CubiertasId,         Name = "Cubiertas",              IsActive = true, IsTireArea = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false },
            new Area { Id = BateriaId,           Name = "Batería",                IsActive = true, IsBatteryArea = true, CreatedAt = seedDate, UpdatedAt = seedDate, IsDeleted = false }
        );
    }
}
