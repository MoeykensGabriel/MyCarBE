using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.WorkOrders.Commands.AddServiceToWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;
using MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.RemoveServiceFromWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderNotes;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrders;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/work-orders")]
[Authorize]
public class WorkOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public WorkOrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista órdenes filtradas por vehicleId, customerId o fleetId.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? vehicleId,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? fleetId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkOrdersQuery(vehicleId, customerId, fleetId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Detalle completo de una orden: servicios, fotos y timeline de estados.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetWorkOrderByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Abre una nueva orden de trabajo para un vehículo. Estado inicial: Received. Solo Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Avanza o cancela el estado de la orden siguiendo la máquina de estados.
    /// Note obligatoria al cancelar. Solo Admin.
    /// Transiciones válidas:
    /// Received → Diagnosing | Cancelled
    /// Diagnosing → AwaitingApproval | Cancelled
    /// AwaitingApproval → InProgress | Cancelled
    /// InProgress → Completed | Cancelled
    /// Completed → Delivered | Cancelled
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeWorkOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId)
            return BadRequest("Route id does not match body workOrderId.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Actualiza las notas del cliente y/o del técnico. Solo Admin.
    /// </summary>
    [HttpPatch("{id:guid}/notes")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotes(
        Guid id,
        [FromBody] UpdateWorkOrderNotesCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId)
            return BadRequest("Route id does not match body workOrderId.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Agrega un servicio del catálogo a la orden. Toma snapshot del nombre, descripción y precio.
    /// Solo Admin.
    /// </summary>
    [HttpPost("{id:guid}/services")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddService(
        Guid id,
        [FromBody] AddServiceToWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId)
            return BadRequest("Route id does not match body workOrderId.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) un servicio de la orden y recalcula el total. Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}/services/{serviceId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveService(
        Guid id,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RemoveServiceFromWorkOrderCommand(id, serviceId), cancellationToken);
        return Ok(result);
    }
}
