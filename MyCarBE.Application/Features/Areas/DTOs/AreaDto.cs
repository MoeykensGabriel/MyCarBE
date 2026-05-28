namespace MyCarBE.Application.Features.Areas.DTOs;

public record AreaDto(
    Guid     Id,
    string   Name,
    bool     IsActive,
    DateTime CreatedAt
);
