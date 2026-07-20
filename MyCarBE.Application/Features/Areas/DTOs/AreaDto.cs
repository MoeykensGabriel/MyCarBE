namespace MyCarBE.Application.Features.Areas.DTOs;

public record AreaDto(
    Guid     Id,
    string   Name,
    bool     IsActive,
    bool     IsTireArea,
    bool     IsBatteryArea,
    bool     IsOilArea,
    DateTime CreatedAt
);
