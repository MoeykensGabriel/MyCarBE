using Mapster;
using MyCarBE.Application.Features.Vehicles.DTOs;
using MyCarBE.Application.Features.Vehicles.Mappings;
using MyCarBE.Domain.Entities;
using Xunit;

namespace MyCarBE.Application.Tests.Vehicles;

/// <summary>
/// El mapeo Vehicle → VehicleDto.
///
/// VehicleDto es un record POSICIONAL de 21 parámetros con tres propiedades init declaradas
/// aparte en el cuerpo (MileageUpdatedAt y las dos derivadas). Cuando Mapster arma el DTO por
/// constructor, las del cuerpo son las que se pueden quedar sin llenar — y si MileageUpdatedAt
/// llega en null, MileageStaleness marca a TODOS los vehículos como "necesita actualizar el
/// kilometraje", que es lo que hacía el aviso del Inicio.
/// </summary>
public class VehicleDtoMappingTests
{
    private static TypeAdapterConfig BuildConfig()
    {
        var config = new TypeAdapterConfig();
        new MyCarBE.Application.Common.Mappings.MappingConfig().Register(config);
        new VehicleMappings().Register(config);
        return config;
    }

    [Fact]
    public void MileageUpdatedAt_LlegaAlDto()
    {
        var lastAt = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);
        var vehicle = new Vehicle
        {
            LicensePlate                     = "AA123BB",
            Brand                            = "Toyota",
            Model                            = "Hilux",
            Year                             = 2020,
            CurrentMileage                   = 45_000,
            MileageUpdatedAt                 = lastAt,
            RegistrationHolderFirstName      = "Juan",
            RegistrationHolderLastName       = "Perez",
            RegistrationHolderDocumentNumber = "20123456789",
        };

        var dto = vehicle.Adapt<VehicleDto>(BuildConfig());

        Assert.Equal(45_000, dto.CurrentMileage);   // control: el constructor sí se llena
        Assert.Equal(lastAt, dto.MileageUpdatedAt); // lo que importa
    }

    [Fact]
    public void MileageUpdatedAt_TambienLlegaMapeandoUnaLISTA()
    {
        // El handler del listado mapea IReadOnlyList<VehicleDto>, no un DTO suelto, y Mapster
        // compila ese camino por separado. Si acá se pierde la fecha, el aviso del Inicio
        // marca a todos los vehículos como vencidos.
        var lastAt = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);
        var vehicles = new List<Vehicle>
        {
            new()
            {
                LicensePlate                     = "AA123BB",
                Brand                            = "Toyota",
                Model                            = "Hilux",
                Year                             = 2020,
                CurrentMileage                   = 45_000,
                MileageUpdatedAt                 = lastAt,
                RegistrationHolderFirstName      = "Juan",
                RegistrationHolderLastName       = "Perez",
                RegistrationHolderDocumentNumber = "20123456789",
            },
        };

        var dtos = vehicles.Adapt<IReadOnlyList<VehicleDto>>(BuildConfig());

        Assert.Equal(45_000, dtos[0].CurrentMileage);
        Assert.Equal(lastAt, dtos[0].MileageUpdatedAt);
    }

    [Fact]
    public void SinLectura_MileageUpdatedAtQuedaEnNull()
    {
        var vehicle = new Vehicle
        {
            LicensePlate                     = "AA123BB",
            Brand                            = "Toyota",
            Model                            = "Hilux",
            Year                             = 2020,
            CurrentMileage                   = 0,
            MileageUpdatedAt                 = null,
            RegistrationHolderFirstName      = "Juan",
            RegistrationHolderLastName       = "Perez",
            RegistrationHolderDocumentNumber = "20123456789",
        };

        var dto = vehicle.Adapt<VehicleDto>(BuildConfig());

        Assert.Null(dto.MileageUpdatedAt);
    }
}
