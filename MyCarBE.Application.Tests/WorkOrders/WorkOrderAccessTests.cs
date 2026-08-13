using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Security;
using MyCarBE.Domain.Entities;
using Xunit;

namespace MyCarBE.Application.Tests.WorkOrders;

/// <summary>
/// Tests de quién puede ver una orden de trabajo.
///
/// Existen porque esta regla estaba escrita tres veces —detalle, PDF del presupuesto e
/// informe de cierre— y las tres habían divergido sin que nadie lo decidiera. Un permiso
/// mal puesto acá no rompe nada visible: simplemente alguien ve de más, o de menos, y no se
/// entera hasta mucho después. Por eso la regla vive en un solo lugar y se testea sola.
///
/// El caso que más importa es el de abajo del todo: un cliente pidiendo la orden de otro
/// cliente tiene que recibir 404 y no 403, porque un 403 le confirma que esa orden existe.
/// </summary>
public class WorkOrderAccessTests
{
    // ── Doble de ICurrentUserService ─────────────────────────────────────────────
    // A propósito NO implementa IsStaff: queremos ejercitar la implementación por
    // defecto de la interfaz, que es la que corre en producción.
    private sealed class Usuario : ICurrentUserService
    {
        public Guid   UserId          { get; init; } = Guid.NewGuid();
        public string Email           { get; init; } = "test@taller.com";
        public string Role            { get; init; } = "";
        public bool   IsAdmin         { get; init; }
        public bool   IsMechanic      { get; init; }
        public bool   IsReceptionist  { get; init; }
        public bool   IsAuthenticated { get; init; } = true;
        public Guid?  CustomerId      { get; init; }
        public Guid?  FleetId         { get; init; }
        public Guid?  MechanicId      { get; init; }
    }

    private static readonly Guid ClienteId = Guid.NewGuid();
    private static readonly Guid FlotaId   = Guid.NewGuid();

    private static Usuario Admin          => new() { IsAdmin = true,        Role = "Admin" };
    private static Usuario Recepcionista  => new() { IsReceptionist = true, Role = "Receptionist" };
    private static Usuario Mecanico       => new() { IsMechanic = true,     Role = "Mechanic", MechanicId = Guid.NewGuid() };
    private static Usuario Cliente(Guid id) => new() { Role = "Customer", CustomerId = id };
    private static Usuario ContactoDeFlota(Guid customerId, Guid fleetId) =>
        new() { Role = "Customer", CustomerId = customerId, FleetId = fleetId };

    private static WorkOrder OrdenDe(Guid? customerId, Guid? fleetId = null) =>
        new() { Id = Guid.NewGuid(), CustomerIdAtEntry = customerId, FleetIdAtEntry = fleetId };

    // ── La oficina ve todo el taller ─────────────────────────────────────────────

    [Fact]
    public void El_admin_ve_cualquier_orden()
    {
        Assert.True(WorkOrderAccess.CanView(OrdenDe(ClienteId), Admin));
    }

    [Fact]
    public void El_recepcionista_ve_cualquier_orden_no_solo_la_que_creo()
    {
        // Es el caso que estaba roto: la recepción atiende el mostrador y necesita abrir
        // la orden de cualquier cliente, no solamente las que dio de alta ella.
        Assert.True(WorkOrderAccess.CanView(OrdenDe(ClienteId), Recepcionista));
    }

    [Fact]
    public void El_recepcionista_ve_tambien_las_ordenes_de_flota()
    {
        Assert.True(WorkOrderAccess.CanView(OrdenDe(null, FlotaId), Recepcionista));
    }

    // ── El cliente ve lo suyo ────────────────────────────────────────────────────

    [Fact]
    public void El_cliente_ve_su_propia_orden()
    {
        Assert.True(WorkOrderAccess.CanView(OrdenDe(ClienteId), Cliente(ClienteId)));
    }

    [Fact]
    public void El_cliente_NO_ve_la_orden_de_otro_cliente()
    {
        var otro = Guid.NewGuid();
        Assert.False(WorkOrderAccess.CanView(OrdenDe(ClienteId), Cliente(otro)));
    }

    [Fact]
    public void El_contacto_de_una_flota_ve_las_ordenes_de_su_flota()
    {
        // La orden entró a nombre de la empresa, no de la persona: el vínculo es la flota.
        var contacto = ContactoDeFlota(Guid.NewGuid(), FlotaId);
        Assert.True(WorkOrderAccess.CanView(OrdenDe(null, FlotaId), contacto));
    }

    [Fact]
    public void El_contacto_de_una_flota_NO_ve_las_ordenes_de_otra_flota()
    {
        var contacto = ContactoDeFlota(Guid.NewGuid(), Guid.NewGuid());
        Assert.False(WorkOrderAccess.CanView(OrdenDe(null, FlotaId), contacto));
    }

    // ── El mecánico no entra por esta puerta ─────────────────────────────────────

    [Fact]
    public void El_mecanico_NO_ve_ordenes_por_esta_via()
    {
        // No es un descuido: el mecánico llega a su trabajo por "mis tareas" y "mis
        // inspecciones", que le muestran solo los servicios y áreas que le tocan.
        Assert.False(WorkOrderAccess.CanView(OrdenDe(ClienteId), Mecanico));
    }

    // ── Leak prevention ──────────────────────────────────────────────────────────

    [Fact]
    public void Pedir_una_orden_ajena_responde_NotFound_y_no_Forbidden()
    {
        // Un 403 le confirmaría a quien prueba ids que esa orden existe. El 404 no.
        var orden = OrdenDe(ClienteId);

        Assert.Throws<NotFoundException>(
            () => WorkOrderAccess.EnsureCanView(orden, Cliente(Guid.NewGuid())));
    }

    [Fact]
    public void Ver_una_orden_propia_no_lanza()
    {
        var orden = OrdenDe(ClienteId);

        WorkOrderAccess.EnsureCanView(orden, Cliente(ClienteId));
    }

    // ── Bordes ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Una_orden_sin_dueño_solo_la_ve_la_oficina()
    {
        // No debería existir, pero si una orden queda sin cliente ni flota, un usuario
        // con CustomerId nulo no puede colarse por comparar null contra null.
        var huerfana = OrdenDe(null, null);

        Assert.True(WorkOrderAccess.CanView(huerfana, Admin));
        Assert.False(WorkOrderAccess.CanView(huerfana, Mecanico));
        Assert.False(WorkOrderAccess.CanView(huerfana, new Usuario { Role = "Customer" }));
    }
}
