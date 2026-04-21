using MediatR;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Commands.UpdateCustomer;

/// <summary>
/// Updates mutable customer fields.
/// DocumentType and DocumentNumber are immutable after creation (identity document).
/// </summary>
public record UpdateCustomerCommand(
    Guid    Id,
    string  FirstName,
    string  LastName,
    string  Phone,
    string? Email
) : IRequest<CustomerDto>;
