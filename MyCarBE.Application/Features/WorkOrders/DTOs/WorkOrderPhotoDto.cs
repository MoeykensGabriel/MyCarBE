using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.DTOs;

public record WorkOrderPhotoDto(
    Guid      Id,
    PhotoType PhotoType,
    string    Url,
    string?   Caption,
    DateTime  TakenAt
);
