using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Common.Security;

/// <summary>
/// Quién puede VER una orden de trabajo: el detalle, el PDF del presupuesto y el informe
/// de cierre.
///
/// La regla vive acá y en un solo lugar porque antes estaba escrita tres veces —una por
/// handler— y las tres habían divergido: el recepcionista podía ver el detalle de la orden
/// que él mismo creó, no podía bajar su presupuesto, y sí podía bajar el informe de cierre
/// de cualquier orden del taller. Nadie decidió eso; se fue desincronizando de a un handler
/// por vez.
///
/// Lo que NO decide este guard: si el documento tiene sentido pedirlo (una orden en
/// diagnóstico todavía no tiene presupuesto) ni las variantes internas del taller (el
/// informe de cierre interno expone costos y sigue siendo solo del Admin). Eso es regla
/// de negocio de cada handler y queda en cada handler.
/// </summary>
public static class WorkOrderAccess
{
    /// <summary>
    /// La oficina (Admin y recepción) ve todo el taller. El cliente ve lo suyo, sea porque
    /// la orden entró a su nombre o porque entró a nombre de su flota. El mecánico no entra
    /// por acá: su pantalla se arma con las queries de "mis tareas" y "mis inspecciones".
    /// </summary>
    public static bool CanView(Guid? customerIdAtEntry, Guid? fleetIdAtEntry, ICurrentUserService user)
    {
        if (user.IsStaff) return true;

        if (user.CustomerId.HasValue && customerIdAtEntry == user.CustomerId) return true;
        if (user.FleetId.HasValue    && fleetIdAtEntry    == user.FleetId)    return true;

        return false;
    }

    public static bool CanView(WorkOrder workOrder, ICurrentUserService user) =>
        CanView(workOrder.CustomerIdAtEntry, workOrder.FleetIdAtEntry, user);

    /// <summary>
    /// Corta el handler si el usuario no puede ver la orden.
    ///
    /// Responde 404 y no 403 a propósito: un 403 le confirma a quien prueba ids ajenos que
    /// esa orden existe. Mismo criterio que VehicleOwnershipGuard. Antes los dos PDF tiraban
    /// 403 acá — el front no distingue uno de otro, así que el cambio no se nota en pantalla.
    /// </summary>
    public static void EnsureCanView(WorkOrder workOrder, ICurrentUserService user)
    {
        if (!CanView(workOrder, user))
            throw new NotFoundException(nameof(WorkOrder), workOrder.Id);
    }
}
