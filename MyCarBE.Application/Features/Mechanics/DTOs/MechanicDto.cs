using MyCarBE.Application.Features.Areas.DTOs;

namespace MyCarBE.Application.Features.Mechanics.DTOs;

public record MechanicDto(
    Guid               Id,
    string             FirstName,
    string             LastName,
    string             Email,
    string?            Phone,
    string?            Specialty,
    bool               IsActive,
    Guid               ApplicationUserId,
    DateTime           CreatedAt,
    IReadOnlyList<AreaDto> Areas
);
