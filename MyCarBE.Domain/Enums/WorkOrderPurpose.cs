namespace MyCarBE.Domain.Enums;

/// <summary>
/// Para qué entró el vehículo al taller. Se declara en el ingreso y NO se reescribe
/// después (ver WorkOrder.Purpose).
/// </summary>
public enum WorkOrderPurpose
{
    /// <summary>
    /// Orden de trabajo normal: se inspecciona, se cotiza y se arregla.
    /// Es el default — las órdenes que ya existían quedan acá sin necesidad de backfill.
    /// </summary>
    Repair = 0,

    /// <summary>
    /// El cliente solo quiere SABER QUÉ TIENE el vehículo. Se inspecciona y se le entrega
    /// el resultado; no se presupuesta ni se arregla. Al cerrar la inspección la orden se
    /// completa sin pasar por Diagnosing.
    ///
    /// El precio de la inspección se arregla con el cliente por fuera del sistema.
    ///
    /// Si después acepta arreglar lo encontrado, la orden se promueve a orden de trabajo
    /// (ver WorkOrder.PromoteToRepair) reusando los hallazgos y propuestas ya cargados.
    /// </summary>
    InspectionOnly = 1,
}
