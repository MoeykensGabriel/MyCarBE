using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.StockRequests.DTOs;

public record StockRequestDto(
    Guid                    Id,
    Guid                    WorkOrderId,
    string                  LicensePlate,
    string?                 ExternalReference,
    StockRequestStatus      Status,
    DateTime                CreatedAt,
    DateTime                UpdatedAt,
    string?                 VehicleBrand,
    string?                 VehicleModel,
    IReadOnlyList<StockRequestItemDto> Items);
