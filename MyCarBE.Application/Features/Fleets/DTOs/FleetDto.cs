namespace MyCarBE.Application.Features.Fleets.DTOs;

public record FleetDto(
    Guid     Id,
    string   CompanyName,
    string   TaxId,
    string   Phone,
    string?  Email,
    string?  Address,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
