namespace MyCarBE.Application.Features.Fleets.DTOs;

/// <summary>
/// Fleet detail returned by GetById. Includes summarized lists of contacts and vehicles
/// so the frontend can render a complete fleet profile in a single request.
/// </summary>
public record FleetDetailDto(
    Guid                               Id,
    string                             CompanyName,
    string                             TaxId,
    string                             Phone,
    string?                            Email,
    string?                            Address,
    DateTime                           CreatedAt,
    DateTime                           UpdatedAt,
    IReadOnlyList<FleetContactSummary> Contacts,
    IReadOnlyList<FleetVehicleSummary> Vehicles
);

public record FleetContactSummary(
    Guid    Id,
    string  FirstName,
    string  LastName,
    string  Phone,
    string? Email
);

public record FleetVehicleSummary(
    Guid   Id,
    string LicensePlate,
    string Brand,
    string Model,
    int    Year
);
