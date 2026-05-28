using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.StockRequests.DTOs;

public record StockRequestItemDto(
    Guid                    Id,
    Guid                    WorkOrderPartId,
    string                  ProductCode,
    string                  Name,
    int                     Quantity,
    StockRequestItemStatus  Status,
    string?                 Notes,
    DateTime                UpdatedAt);
