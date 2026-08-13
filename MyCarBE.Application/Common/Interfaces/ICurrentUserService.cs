namespace MyCarBE.Application.Common.Interfaces;

/// <summary>
/// Provee información del usuario autenticado en el request actual.
/// Implementado en la capa API usando IHttpContextAccessor.
/// Los Handlers lo inyectan para saber quién ejecuta la operación.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsAdmin { get; }
    bool IsMechanic { get; }
    bool IsReceptionist { get; }
    bool IsAuthenticated { get; }

    /// <summary>
    /// Personal de la OFICINA: administra el taller de punta a punta — ve todas las órdenes,
    /// todos los vehículos y todos los clientes, sin importar de quién sean.
    ///
    /// Es el permiso que hay que consultar para LEER la operación del taller. No alcanza con
    /// preguntar IsAdmin: el recepcionista atiende el mostrador y necesita ver lo mismo.
    /// Para lo que sí es exclusivo del dueño (dashboard, ventas y comisiones, configuración,
    /// alta de usuarios, borrado) se sigue preguntando IsAdmin.
    /// </summary>
    bool IsStaff => IsAdmin || IsReceptionist;

    /// <summary>
    /// Id del Customer vinculado al usuario. Null si el usuario es Admin o Mechanic.
    /// </summary>
    Guid? CustomerId { get; }

    /// <summary>
    /// Id de la flota a la que pertenece el Customer. Null si es un particular, un Admin o un Mechanic.
    /// </summary>
    Guid? FleetId { get; }

    /// <summary>
    /// Id del Mechanic vinculado al usuario. No depende del rol sino de tener un perfil de
    /// ejecutante activo: lo tiene todo Mechanic, y también el Admin, a quien se le crea el
    /// suyo en el primer login para que pueda hacer trabajos con sus propias manos.
    /// </summary>
    Guid? MechanicId { get; }
}
