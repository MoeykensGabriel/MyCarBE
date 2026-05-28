using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.VehicleTires.Commands.AddTireMeasurement;
using MyCarBE.Application.Features.VehicleTires.Commands.CreateVehicleTire;
using MyCarBE.Application.Features.VehicleTires.Commands.ReplaceTire;
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Application.Features.VehicleTires.Queries.GetTiresByVehicle;
using MyCarBE.Domain.Enums;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Cubiertas (neumáticos) por posición de un vehículo, sus mediciones y
/// estimaciones de desgaste/cambio.
///
/// Lecturas: las hace el handler con VehicleOwnershipGuard (Admin / Customer dueño /
/// Fleet Contact). No hace falta restringir por rol en el endpoint — el guard ya filtra.
///
/// Escrituras (crear cubierta, agregar medición, reemplazar): solo personal del taller.
/// Restringido por rol acá: Admin, Receptionist, Mechanic.
/// </summary>
[ApiController]
[Authorize]
public class VehicleTiresController : ControllerBase
{
    private const string WorkshopRoles = "Admin,Receptionist,Mechanic";

    private readonly ISender _sender;
    public VehicleTiresController(ISender sender) => _sender = sender;

    // Body records (ids vienen por ruta para no duplicarlos)
    public record CreateTireBody(
        TirePosition Position,
        string       Brand,
        string       Model,
        string       SizeSpec,
        DateOnly     InstalledOn,
        int          InstalledAtKm,
        decimal      InitialTreadDepthMm,
        int          ExpectedLifeKm);

    public record AddMeasurementBody(
        DateTime MeasuredOn,
        int      VehicleMileageAtMeasurement,
        decimal  InnerDepthMm,
        decimal  CenterDepthMm,
        decimal  OuterDepthMm,
        string?  Notes);

    public record ReplaceTireBody(
        DateOnly ReplacedOn,
        int      ReplacedAtKm,
        string   NewBrand,
        string   NewModel,
        string   NewSizeSpec,
        decimal  NewInitialTreadDepthMm,
        int      NewExpectedLifeKm);

    /// <summary>
    /// Lista las cubiertas activas del vehículo con la estimación de desgaste.
    /// Pasar includeReplaced=true para incluir el historial de reemplazadas.
    /// </summary>
    [HttpGet("api/vehicles/{vehicleId:guid}/tires")]
    [ProducesResponseType(typeof(IReadOnlyList<VehicleTireDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(
        Guid vehicleId,
        [FromQuery] bool includeReplaced = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetTiresByVehicleQuery(vehicleId, includeReplaced), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Da de alta una cubierta en una posición del vehículo. Si esa posición ya tiene
    /// una activa, se marca como reemplazada (no se borra).
    /// </summary>
    [HttpPost("api/vehicles/{vehicleId:guid}/tires")]
    [Authorize(Roles = WorkshopRoles)]
    [ProducesResponseType(typeof(VehicleTireDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid vehicleId,
        [FromBody] CreateTireBody body,
        CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new CreateVehicleTireCommand(
            vehicleId,
            body.Position,
            body.Brand,
            body.Model,
            body.SizeSpec,
            body.InstalledOn,
            body.InstalledAtKm,
            body.InitialTreadDepthMm,
            body.ExpectedLifeKm), cancellationToken);

        return CreatedAtAction(nameof(List), new { vehicleId }, dto);
    }

    /// <summary>Agrega una medición de profundidad (3 puntos) a la cubierta indicada.</summary>
    [HttpPost("api/tires/{tireId:guid}/measurements")]
    [Authorize(Roles = WorkshopRoles)]
    [ProducesResponseType(typeof(VehicleTireDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMeasurement(
        Guid tireId,
        [FromBody] AddMeasurementBody body,
        CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new AddTireMeasurementCommand(
            tireId,
            body.MeasuredOn,
            body.VehicleMileageAtMeasurement,
            body.InnerDepthMm,
            body.CenterDepthMm,
            body.OuterDepthMm,
            body.Notes), cancellationToken);

        return Ok(dto);
    }

    /// <summary>
    /// Reemplaza la cubierta indicada: la actual queda en historial y se crea una
    /// nueva activa en la misma posición.
    /// </summary>
    [HttpPost("api/tires/{tireId:guid}/replace")]
    [Authorize(Roles = WorkshopRoles)]
    [ProducesResponseType(typeof(VehicleTireDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Replace(
        Guid tireId,
        [FromBody] ReplaceTireBody body,
        CancellationToken cancellationToken)
    {
        var dto = await _sender.Send(new ReplaceTireCommand(
            tireId,
            body.ReplacedOn,
            body.ReplacedAtKm,
            body.NewBrand,
            body.NewModel,
            body.NewSizeSpec,
            body.NewInitialTreadDepthMm,
            body.NewExpectedLifeKm), cancellationToken);

        return CreatedAtAction(nameof(List), new { vehicleId = dto.VehicleId }, dto);
    }
}
