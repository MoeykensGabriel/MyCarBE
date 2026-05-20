namespace MyCarBE.Application.Features.Customers.DTOs;

/// <summary>
/// Returned only on creation. Contains the customer data plus the temporary password
/// that the receptionist must hand over to the customer for their first login.
/// </summary>
public record CreateCustomerResponseDto(
    CustomerDto Customer,
    string      TempPassword
);
