using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.WorkOrders.Commands.AddAdHocServiceToWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.AddPartToWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.AddServiceToWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.ApproveAsCustomer;
using MyCarBE.Application.Features.WorkOrders.Commands.ApproveWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.ChangeWorkOrderStatus;
using MyCarBE.Application.Features.WorkOrders.Commands.CloseInspection;
using MyCarBE.Application.Features.WorkOrders.Commands.ConvertInspectionProposals;
using MyCarBE.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.DeleteWorkOrderPhoto;
using MyCarBE.Application.Features.WorkOrders.Commands.RemovePartFromWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.RemoveServiceFromWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.ScheduleWorkOrder;
using MyCarBE.Application.Features.WorkOrders.Commands.SendQuote;
using MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderNotes;
using MyCarBE.Application.Features.WorkOrders.Commands.UpdateWorkOrderPart;
using MyCarBE.Application.Features.WorkOrders.Commands.UploadWorkOrderPhoto;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Application.Features.WorkOrders.Queries.GetApprovalInfo;
using MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderById;
using MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrderQuotePdf;
using MyCarBE.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MyCarBE.Application.Features.InspectionReports.Commands.MarkAreaNoFindings;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Application.Features.InspectionReports.Queries.GetInspectionReportsByWorkOrder;
using MyCarBE.Domain.Enums;

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
    /// Lista órdenes paginadas con filtros combinables:
    /// - vehicleId / customerId / fleetId: scope de entidad
    /// - status: filtra por estado (enum int)
    /// - search: busca por patente, nombre de cliente o razón social de flota (case-insensitive, contains)
    /// Customer ignora los filtros de scope — ve solo sus propias órdenes (por JWT).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WorkOrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?                vehicleId,
        [FromQuery] Guid?                customerId,
        [FromQuery] Guid?                fleetId,
        [FromQuery] WorkOrderStatus?     status,
        [FromQuery] WorkOrderOwnerType?  ownerType,
        [FromQuery] string?              search,
        [FromQuery] int                  page     = 1,
        [FromQuery] int                  pageSize = 20,
        CancellationToken                cancellationToken = default)
    {
        var result = await _sender.Send(new GetWorkOrdersQuery(vehicleId, customerId, fleetId, status, ownerType, search, page, pageSize), cancellationToken);
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
    [Authorize(Roles = "Admin,Receptionist")]
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
    /// Agenda la orden (vehículo) en el calendario de ocupación del taller. Si no se pasa
    /// ScheduledEnd, se calcula como inicio + duración total estimada de los servicios.
    /// Pasar ambos null borra el agendado. Solo Admin.
    /// </summary>
    [HttpPost("{id:guid}/schedule")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule(
        Guid id,
        [FromBody] ScheduleWorkOrderCommand command,
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
    /// Agrega un servicio "puntual" (ad-hoc) a la orden — no requiere catálogo.
    /// Útil para trabajos únicos que no tiene sentido sumar al catálogo permanente.
    /// Solo Admin.
    /// </summary>
    [HttpPost("{id:guid}/services/ad-hoc")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAdHocService(
        Guid id,
        [FromBody] AddAdHocServiceToWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId)
            return BadRequest("Route id does not match body workOrderId.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sube una foto a la orden. Si se pasa workOrderServiceId, la foto queda vinculada
    /// a ese servicio (usado por los mecánicos para documentar el trabajo realizado).
    /// Si no se pasa, es una foto general del vehículo (Before del intake / After de entrega).
    ///
    /// Permisos:
    /// - Admin / Receptionist: pueden subir cualquier foto, con o sin servicio asociado.
    /// - Mechanic: solo puede subir fotos vinculadas a un servicio asignado a él.
    /// </summary>
    [HttpPost("{id:guid}/photos")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(
        Guid id,
        IFormFile file,
        [FromForm] PhotoType photoType,
        [FromForm] string? caption,
        [FromForm] Guid? workOrderServiceId,
        [FromForm] Guid? inspectionReportId,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadWorkOrderPhotoCommand(
            id, photoType, stream, file.FileName, caption, workOrderServiceId, inspectionReportId);
        var result  = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) una foto de la orden. Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(
        Guid id,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteWorkOrderPhotoCommand(id, photoId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Envía el presupuesto al cliente. Congela los items, genera token con TTL 30 días,
    /// transiciona la orden a AwaitingApproval y dispara el email con el link de aprobación.
    /// Solo Admin. Solo desde Diagnosing.
    /// </summary>
    [HttpPost("{id:guid}/send-quote")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendQuote(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SendQuoteCommand(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Descarga el PDF del presupuesto.
    /// - Admin: cualquier orden con presupuesto generado.
    /// - Customer: solo sus propias órdenes (validación de ownership en el handler).
    /// Disponible desde que la orden tiene presupuesto (AwaitingApproval en adelante).
    /// </summary>
    [HttpGet("{id:guid}/quote")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadQuotePdf(Guid id, CancellationToken cancellationToken)
    {
        var pdfBytes = await _sender.Send(new GetWorkOrderQuotePdfQuery(id), cancellationToken);
        return File(pdfBytes, "application/pdf", $"Presupuesto-{id.ToString()[..8].ToUpper()}.pdf");
    }

    /// <summary>
    /// Devuelve el detalle del presupuesto para la página de confirmación del cliente.
    /// Enlace de un solo uso, válido 48 horas. Público (sin autenticación requerida).
    /// </summary>
    [HttpGet("approve")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApprovalInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApprovalInfo(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApprovalInfoQuery(token), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirma la aprobación del presupuesto item-by-item y avanza la orden a Approved.
    /// El cliente selecciona qué items aprueba; los no incluidos quedan como Rejected.
    /// Reglas: cada AlternativeGroupId requiere exactamente 1 elección, mínimo 1 item aprobado.
    /// Enlace de un solo uso, válido 30 días. Público (sin autenticación requerida).
    /// </summary>
    [HttpPost("approve")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        [FromBody] ApproveWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Aprobación item-by-item desde el panel del cliente logueado (sin token).
    /// Valida ownership (directo o vía flota) y aplica las mismas reglas que la versión pública.
    /// </summary>
    [HttpPost("{id:guid}/approve-as-customer")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveAsCustomer(
        Guid id,
        [FromBody] ApproveAsCustomerCommand command,
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

    // ── Repuestos (parts) ─────────────────────────────────────────────────────
    // Solo editables durante Diagnosing — antes de enviar el presupuesto al cliente.
    // ProductCode es opcional: si es null el repuesto no se envía a GestionPGB.

    /// <summary>
    /// Agrega un repuesto a la orden durante la cotización. Solo Admin, solo en Diagnosing.
    /// </summary>
    [HttpPost("{id:guid}/parts")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddPart(
        Guid id,
        [FromBody] AddPartToWorkOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId)
            return BadRequest("Route id does not match body workOrderId.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Convierte propuestas de inspección (sugeridas por los mecánicos) en items reales del presupuesto.
    /// Solo Admin, solo en Diagnosing. El admin elige cuáles propuestas pasan al presupuesto.
    /// </summary>
    [HttpPost("{id:guid}/convert-proposals")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConvertProposals(
        Guid id,
        [FromBody] ConvertInspectionProposalsCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId)
            return BadRequest("Route id does not match body workOrderId.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Edita un repuesto. Solo Admin, solo en Diagnosing, solo si no fue congelado.
    /// </summary>
    [HttpPatch("{id:guid}/parts/{partId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePart(
        Guid id,
        Guid partId,
        [FromBody] UpdateWorkOrderPartCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.WorkOrderId || partId != command.PartId)
            return BadRequest("Route ids do not match body ids.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) un repuesto. Solo Admin, solo en Diagnosing, solo si no fue congelado.
    /// </summary>
    [HttpDelete("{id:guid}/parts/{partId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePart(
        Guid id,
        Guid partId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RemovePartFromWorkOrderCommand(id, partId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Lista los reportes de inspección consolidados por área de una orden.
    /// Para el panel admin que decide si cerrar la inspección y pasar a cotizar.
    /// </summary>
    [HttpGet("{id:guid}/inspection-reports")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<InspectionReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInspectionReports(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInspectionReportsByWorkOrderQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// El admin marca un área como "sin hallazgos" (útil cuando no hay mecánico
    /// asignado al área o nadie llegó a reportar). Crea un InspectionReport con IsNoFindings=true.
    /// </summary>
    [HttpPost("{id:guid}/inspection-reports/no-findings")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(InspectionReportDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkAreaNoFindings(
        Guid id,
        [FromBody] MarkAreaNoFindingsBody body,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new MarkAreaNoFindingsCommand(id, body.AreaId), cancellationToken);
        return Ok(result);
    }

    public record MarkAreaNoFindingsBody(Guid AreaId);

    /// <summary>
    /// El admin cierra la inspección colectiva — requiere que todas las áreas activas
    /// tengan un reporte (de mecánico o sin hallazgos). Transiciona a Diagnosing.
    /// </summary>
    [HttpPost("{id:guid}/close-inspection")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(WorkOrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseInspection(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CloseInspectionCommand(id), cancellationToken);
        return Ok(result);
    }
}
