using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderStatusChangeDto(
    Guid             Id,
    WorkOrderStatus? FromStatus,
    WorkOrderStatus  ToStatus,
    DateTime         ChangedAt,
    Guid             ChangedByUserId,
    string?          Note
);
