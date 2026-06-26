using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Sales.Commands.CreateSale;
using MyCarBE.Application.Features.Sales.DTOs;
using MyCarBE.Application.Features.Sales.Queries.GetSaleById;
using MyCarBE.Application.Features.Sales.Queries.GetSales;

namespace MyCarBE.API.Controllers;

/// <summary>
/// Ventas de repuestos "de mostrador" (sin orden ni vehículo). Solo office: Admin / Recepcionista.
/// </summary>
[ApiController]
[Route("api/sales")]
[Authorize(Roles = "Admin,Receptionist")]
public class SalesController : ControllerBase
{
    private readonly ISender _sender;

    public SalesController(ISender sender) => _sender = sender;

    /// <summary>Registra una venta. El vendedor es el usuario logueado.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Lista paginada de ventas, filtrable por cliente/flota/vendedor y rango de fechas.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SaleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid?     customerId,
        [FromQuery] Guid?     fleetId,
        [FromQuery] Guid?     sellerUserId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int       page     = 1,
        [FromQuery] int       pageSize = 20,
        CancellationToken     cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetSalesQuery(customerId, fleetId, sellerUserId, from, to, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Detalle de una venta.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSaleByIdQuery(id), cancellationToken);
        return Ok(result);
    }
}
