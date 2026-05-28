namespace MyCarBE.Domain.Enums;

/// <summary>
/// Tipo de documento del vehículo con fecha de vencimiento. El cliente carga
/// y el sistema alerta cuando se acercan al vencimiento.
///
/// Se agregan valores al final para no renumerar los existentes en BD.
/// </summary>
public enum VehicleDocumentType
{
    /// <summary>Verificación Técnica Vehicular (VTV / ITV / RTO según país).</summary>
    TechnicalInspection = 0,

    /// <summary>Póliza de seguro.</summary>
    Insurance           = 1,

    /// <summary>Patentamiento / impuesto automotor.</summary>
    Registration        = 2,

    /// <summary>Revisión de emisiones (cuando aplica por separado de VTV).</summary>
    EmissionTest        = 3,

    /// <summary>Otro tipo de documento con vencimiento (texto libre en Notes).</summary>
    Other               = 99,
}
