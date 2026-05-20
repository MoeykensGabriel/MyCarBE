using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCarBE.Application.Features.Customers.Commands.CreateCustomer;
using MyCarBE.Application.Features.Customers.Commands.DeleteCustomer;
using MyCarBE.Application.Features.Customers.Commands.UpdateCustomer;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Customers.DTOs;
using MyCarBE.Application.Features.Customers.Queries.GetAllCustomers;
using MyCarBE.Application.Features.Customers.Queries.GetCurrentCustomer;
using MyCarBE.Application.Features.Customers.Queries.GetCustomerById;

namespace MyCarBE.API.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Devuelve el perfil del customer autenticado.
    /// Usado por el portal del cliente — no requiere conocer el Id.
    /// </summary>
    [HttpGet("me")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCurrentCustomerQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna todos los clientes. Opcionalmente filtra por nombre, apellido, documento o teléfono.
    /// Solo Admin.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        CancellationToken   cancellationToken = default)
    {
        var result = await _sender.Send(new GetAllCustomersQuery(search, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna un cliente por Id. Admin o el propio cliente.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCustomerByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Crea un nuevo cliente y su acceso al portal. Retorna los datos del cliente y la contraseña temporal.
    /// Solo Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    [ProducesResponseType(typeof(CreateCustomerResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Customer.Id }, result);
    }

    /// <summary>
    /// Actualiza los datos editables de un cliente (nombre, apellido, teléfono, email).
    /// Solo Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match body id.");

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Elimina (soft delete) un cliente. Solo Admin.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCustomerCommand(id), cancellationToken);
        return NoContent();
    }
}
