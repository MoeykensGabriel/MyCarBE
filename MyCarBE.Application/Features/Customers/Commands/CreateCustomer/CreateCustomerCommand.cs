using MediatR;
using MyCarBE.Application.Features.Customers.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Customers.Commands.CreateCustomer;

/// <summary>
/// Creates a Customer entity and an associated ApplicationUser (Customer role).
/// Returns the new customer data together with the generated temporary password.
/// </summary>
public record CreateCustomerCommand(
    string       FirstName,
    string       LastName,
    DocumentType DocumentType,
    string       DocumentNumber,
    string       Phone,
    string       Email,
    Guid?        FleetId = null
) : IRequest<CreateCustomerResponseDto>;
