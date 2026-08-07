using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;
using Xunit;

namespace MyCarBE.Application.Tests.WorkOrders;

/// <summary>
/// Tests de las órdenes de SOLO INSPECCIÓN: el cliente solo quiere saber qué tiene el
/// vehículo, no arreglarlo. Esa orden no pasa por Diagnosing — al cerrar la inspección se
/// completa — y si después el cliente acepta arreglar, se promueve a orden de trabajo
/// reusando los hallazgos ya cargados.
///
/// El caso más importante que se cubre acá es el inverso: habilitar Completed → Diagnosing
/// para la promoción abre la puerta a REABRIR una orden de trabajo ya terminada, lo que
/// descongelaría ítems que el cliente aprobó. Ese guard tiene que aguantar.
///
/// Dominio puro, sin EF ni mocks.
/// </summary>
public class InspectionOnlyOrderTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static WorkOrder OrdenEnInspeccion(WorkOrderPurpose purpose) =>
        new()
        {
            Purpose       = purpose,
            CurrentStatus = WorkOrderStatus.UnderInspection,
        };

    /// <summary>Orden de solo inspección ya cerrada, lista para promover.</summary>
    private static WorkOrder InspeccionCerrada()
    {
        var wo = OrdenEnInspeccion(WorkOrderPurpose.InspectionOnly);
        wo.ChangeStatus(WorkOrderStatus.Completed, UserId);
        return wo;
    }

    // ── Cierre ───────────────────────────────────────────────────────────────

    [Fact]
    public void Una_orden_de_solo_inspeccion_se_cierra_desde_la_inspeccion()
    {
        var wo = OrdenEnInspeccion(WorkOrderPurpose.InspectionOnly);

        wo.ChangeStatus(WorkOrderStatus.Completed, UserId);

        Assert.Equal(WorkOrderStatus.Completed, wo.CurrentStatus);
    }

    [Fact]
    public void Una_orden_de_trabajo_NO_se_cierra_desde_la_inspeccion()
    {
        var wo = OrdenEnInspeccion(WorkOrderPurpose.Repair);

        // Tiene que pasar por Diagnosing y cotizar: saltear eso dejaría una orden
        // terminada sin presupuesto ni aprobación del cliente.
        Assert.Throws<InvalidOperationException>(
            () => wo.ChangeStatus(WorkOrderStatus.Completed, UserId));
    }

    [Fact]
    public void Una_orden_de_solo_inspeccion_NO_pasa_a_cotizacion()
    {
        var wo = OrdenEnInspeccion(WorkOrderPurpose.InspectionOnly);

        Assert.Throws<InvalidOperationException>(
            () => wo.ChangeStatus(WorkOrderStatus.Diagnosing, UserId));
    }

    [Fact]
    public void Una_orden_de_solo_inspeccion_no_acepta_trabajo()
    {
        var wo = OrdenEnInspeccion(WorkOrderPurpose.InspectionOnly);

        Assert.False(wo.AcceptsNewWork);
    }

    // ── El guard crítico ─────────────────────────────────────────────────────

    [Fact]
    public void Una_orden_de_trabajo_completada_NO_puede_volver_a_cotizacion()
    {
        var wo = new WorkOrder
        {
            Purpose       = WorkOrderPurpose.Repair,
            CurrentStatus = WorkOrderStatus.Completed,
        };

        // Si esto pasara, los ítems que el cliente ya aprobó volverían a ser editables
        // y RecalculateTotalAmount cambiaría de criterio a mitad del ciclo.
        Assert.Throws<InvalidOperationException>(
            () => wo.ChangeStatus(WorkOrderStatus.Diagnosing, UserId));
    }

    [Fact]
    public void No_se_promueve_una_orden_que_no_es_de_solo_inspeccion()
    {
        var wo = new WorkOrder
        {
            Purpose       = WorkOrderPurpose.Repair,
            CurrentStatus = WorkOrderStatus.Completed,
        };

        Assert.Throws<InvalidOperationException>(() => wo.PromoteToRepair(UserId));
    }

    // ── Promoción ────────────────────────────────────────────────────────────

    [Fact]
    public void Promover_deja_la_orden_en_cotizacion_y_habilitada_para_trabajo()
    {
        var wo = InspeccionCerrada();

        wo.PromoteToRepair(UserId);

        Assert.Equal(WorkOrderStatus.Diagnosing, wo.CurrentStatus);
        Assert.NotNull(wo.PromotedToRepairAt);
        Assert.False(wo.IsInspectionOnly);
        Assert.True(wo.AcceptsNewWork);
    }

    [Fact]
    public void Promover_NO_reescribe_como_entro_la_orden()
    {
        var wo = InspeccionCerrada();

        wo.PromoteToRepair(UserId);

        // Purpose es el registro histórico: la orden ENTRÓ como solo inspección y eso
        // es justamente lo que permite saber después que nunca pasó por diagnóstico.
        Assert.Equal(WorkOrderPurpose.InspectionOnly, wo.Purpose);
    }

    [Fact]
    public void No_se_promueve_dos_veces()
    {
        var wo = InspeccionCerrada();
        wo.PromoteToRepair(UserId);

        Assert.Throws<InvalidOperationException>(() => wo.PromoteToRepair(UserId));
    }

    [Fact]
    public void No_se_promueve_una_inspeccion_todavia_abierta()
    {
        var wo = OrdenEnInspeccion(WorkOrderPurpose.InspectionOnly);

        Assert.Throws<InvalidOperationException>(() => wo.PromoteToRepair(UserId));
    }

    // ── Órdenes históricas ───────────────────────────────────────────────────

    [Fact]
    public void Una_orden_sin_proposito_declarado_es_orden_de_trabajo()
    {
        var wo = new WorkOrder();

        // Es el estado en que quedan todas las órdenes anteriores a la migración.
        Assert.Equal(WorkOrderPurpose.Repair, wo.Purpose);
        Assert.False(wo.IsInspectionOnly);
    }
}
