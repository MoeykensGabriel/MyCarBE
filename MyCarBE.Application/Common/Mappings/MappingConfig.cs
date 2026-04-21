using Mapster;

namespace MyCarBE.Application.Common.Mappings;

/// <summary>
/// Configuración global de Mapster. Implementa IRegister para que Mapster
/// escanee el assembly automáticamente y registre todos los mappings.
/// Cada Feature agrega su propio IRegister cuando se construye.
/// </summary>
public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configuración global: ignorar propiedades de auditoría en respuestas
        config.Default
            .IgnoreNullValues(true);

        // Los mappings específicos por entidad se agregan en cada Feature:
        // Customers/Mappings/CustomerMappings.cs : IRegister
        // Vehicles/Mappings/VehicleMappings.cs   : IRegister
        // etc.
    }
}
