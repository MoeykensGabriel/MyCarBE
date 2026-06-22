using Mapster;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.WorkOrders.Mappings;

public class WorkOrderMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WorkOrder, WorkOrderSummaryDto>()
            .Map(dest => dest.VehicleBrand,        src => src.Vehicle.Brand)
            .Map(dest => dest.VehicleModel,        src => src.Vehicle.Model)
            .Map(dest => dest.VehicleLicensePlate, src => src.Vehicle.LicensePlate)
            .Map(dest => dest.OwnerName,           src =>
                src.CustomerAtEntry != null
                    ? $"{src.CustomerAtEntry.FirstName} {src.CustomerAtEntry.LastName}"
                    : src.FleetAtEntry != null
                        ? src.FleetAtEntry.CompanyName
                        : null);

        config.NewConfig<WorkOrderStatusChange, WorkOrderStatusChangeDto>();

        config.NewConfig<WorkOrderService, WorkOrderServiceDto>()
            .Map(dest => dest.Subtotal, src => src.PriceSnapshot * src.Quantity)
            .Map(dest => dest.AssignedMechanicName, src =>
                src.AssignedMechanic != null
                    ? $"{src.AssignedMechanic.FirstName} {src.AssignedMechanic.LastName}"
                    : null)
            // Duración del snapshot — soporta servicios del catálogo y ad-hoc por igual.
            // El handler de AddService snapshotea la duración al crear, así que siempre
            // hay un valor confiable acá (0 solo en datos legacy pre-snapshot).
            .Map(dest => dest.EstimatedDurationMinutes, src => src.EstimatedDurationMinutesSnapshot)
            .Map(dest => dest.AreaName, src => src.Area != null ? src.Area.Name : null);

        config.NewConfig<WorkOrderPhoto, WorkOrderPhotoDto>();

        config.NewConfig<InspectionReport, WorkOrderInspectionReportLiteDto>()
            .Map(d => d.AreaName, s => s.Area.Name)
            .Map(d => d.MechanicFullName, s =>
                s.Mechanic != null
                    ? $"{s.Mechanic.FirstName} {s.Mechanic.LastName}".Trim()
                    : null);

        config.NewConfig<WorkOrderPart, WorkOrderPartDto>()
            .Map(dest => dest.Subtotal, src => src.UnitPrice * src.Quantity);

        config.NewConfig<WorkOrder, WorkOrderDetailDto>()
            .Map(dest => dest.VehicleBrand,        src => src.Vehicle.Brand)
            .Map(dest => dest.VehicleModel,        src => src.Vehicle.Model)
            .Map(dest => dest.VehicleLicensePlate, src => src.Vehicle.LicensePlate)
            .Map(dest => dest.OwnerName,           src =>
                src.CustomerAtEntry != null
                    ? $"{src.CustomerAtEntry.FirstName} {src.CustomerAtEntry.LastName}"
                    : src.FleetAtEntry != null
                        ? src.FleetAtEntry.CompanyName
                        : null)
            .Map(dest => dest.Services,  src => src.Services)
            .Map(dest => dest.Parts,     src => src.Parts)
            .Map(dest => dest.Photos,    src => src.Photos)
            .Map(dest => dest.Timeline,  src => src.StatusChanges)
            // Duración total estimada: prioriza la estimación del mecánico, fallback al catálogo.
            // Se mapea en memoria (la entidad ya viene cargada con sus servicios).
            .Map(dest => dest.TotalEstimatedMinutes, src => src.Services
                .Where(s => !s.IsDeleted)
                .Sum(s => (s.EstimatedDurationMinutes ?? s.EstimatedDurationMinutesSnapshot) * s.Quantity));
    }
}
