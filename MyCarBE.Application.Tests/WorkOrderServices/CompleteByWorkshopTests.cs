using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;
using Xunit;

namespace MyCarBE.Application.Tests.WorkOrderServices;

/// <summary>
/// Tests de <see cref="WorkOrderService.CompleteByWorkshop"/>: el cierre "en nombre del taller"
/// con el que admin u oficina destraban un trabajo que su mecánico no va a terminar.
///
/// El caso que motivó extenderlo a Pending: un mecánico toma un trabajo y nunca lo arranca.
/// Antes solo se podía cerrar desde Accepted, así que ese Pending abandonado trababa la orden
/// entera — WorkOrder no pasa a Completed mientras quede un servicio sin finalizar.
/// Dominio puro, sin EF ni mocks.
/// </summary>
public class CompleteByWorkshopTests
{
    private const string ValidNotes = "Lo terminó el taller porque el mecánico no volvió.";

    private static WorkOrderService ServiceWith(
        WorkOrderServiceAssignmentStatus status,
        Guid? mechanicId = null,
        DateTime? acceptedAt = null) => new()
        {
            Id                 = Guid.NewGuid(),
            NameSnapshot       = "Cambio de correa",
            AssignmentStatus   = status,
            AssignedMechanicId = mechanicId,
            AcceptedAt         = acceptedAt,
        };

    [Fact]
    public void FromPending_Completes_AndKeepsMechanicForHistory()
    {
        var mechanicId = Guid.NewGuid();
        var service = ServiceWith(WorkOrderServiceAssignmentStatus.Pending, mechanicId);

        service.CompleteByWorkshop(ValidNotes);

        Assert.Equal(WorkOrderServiceAssignmentStatus.Completed, service.AssignmentStatus);
        Assert.NotNull(service.CompletedAt);
        Assert.Equal(ValidNotes, service.MechanicNotes);

        // El mecánico que lo tomó queda registrado aunque no lo haya hecho él.
        Assert.Equal(mechanicId, service.AssignedMechanicId);

        // Nadie lo inició, así que AcceptedAt no se inventa.
        Assert.Null(service.AcceptedAt);
    }

    [Fact]
    public void FromAccepted_Completes_AndPreservesAcceptedAt()
    {
        var mechanicId = Guid.NewGuid();
        var acceptedAt = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        var service = ServiceWith(WorkOrderServiceAssignmentStatus.Accepted, mechanicId, acceptedAt);

        service.CompleteByWorkshop(ValidNotes);

        Assert.Equal(WorkOrderServiceAssignmentStatus.Completed, service.AssignmentStatus);
        Assert.Equal(mechanicId, service.AssignedMechanicId);
        Assert.Equal(acceptedAt, service.AcceptedAt);
    }

    [Fact]
    public void FromUnassigned_Throws()
    {
        var service = ServiceWith(WorkOrderServiceAssignmentStatus.Unassigned);

        var ex = Assert.Throws<InvalidOperationException>(() => service.CompleteByWorkshop(ValidNotes));
        Assert.Contains("asignalo o tomalo primero", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromCompleted_Throws()
    {
        var service = ServiceWith(WorkOrderServiceAssignmentStatus.Completed, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => service.CompleteByWorkshop(ValidNotes));
    }

    [Fact]
    public void WithoutNotes_Throws()
    {
        var service = ServiceWith(WorkOrderServiceAssignmentStatus.Pending, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => service.CompleteByWorkshop("   "));
    }
}
