using MediatR;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Queries.GetCurrentCustomer;

/// <summary>
/// Devuelve el perfil del customer autenticado usando el CustomerId del JWT.
/// Usado por el portal del cliente — no requiere que el frontend conozca el Id.
/// </summary>
public record GetCurrentCustomerQuery : IRequest<CustomerDto>;
