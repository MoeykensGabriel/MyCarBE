using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;

namespace MyCarBE.Application.Features.WorkOrders.Queries.GetApprovalInfo;

public class GetApprovalInfoQueryHandler : IRequestHandler<GetApprovalInfoQuery, ApprovalInfoDto>
{
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly IWorkOrderRepository              _workOrderRepository;

    public GetApprovalInfoQueryHandler(
        IWorkOrderApprovalTokenRepository tokenRepository,
        IWorkOrderRepository              workOrderRepository)
    {
        _tokenRepository     = tokenRepository;
        _workOrderRepository = workOrderRepository;
    }

    public async Task<ApprovalInfoDto> Handle(GetApprovalInfoQuery request, CancellationToken cancellationToken)
    {
        var approvalToken = await _tokenRepository.GetByTokenValueAsync(request.Token, cancellationToken)
            ?? throw new NotFoundException("Approval token", request.Token);

        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(approvalToken.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.WorkOrder), approvalToken.WorkOrderId);

        var services = workOrder.Services
            .Where(s => !s.IsDeleted)
            .Select(s => new ApprovalServiceItemDto(
                Id:          s.Id,
                Name:        s.NameSnapshot,
                Description: s.DescriptionSnapshot,
                UnitPrice:   s.PriceSnapshot,
                Quantity:    s.Quantity,
                Subtotal:    s.PriceSnapshot * s.Quantity))
            .ToList();

        var parts = workOrder.Parts
            .Where(p => !p.IsDeleted)
            .Select(p => new ApprovalPartItemDto(
                Id:          p.Id,
                Name:        p.Name,
                ProductCode: p.ProductCode,
                UnitPrice:   p.UnitPrice,
                Quantity:    p.Quantity,
                Subtotal:    p.Subtotal,
                Tier:        p.Tier))
            .ToList();

        var customerName =
            workOrder.FleetAtEntry?.CompanyName
            ?? (workOrder.CustomerAtEntry is not null
                ? $"{workOrder.CustomerAtEntry.FirstName} {workOrder.CustomerAtEntry.LastName}".Trim()
                : "Cliente");

        return new ApprovalInfoDto(
            WorkOrderId:         workOrder.Id,
            VehicleLicensePlate: workOrder.Vehicle.LicensePlate,
            VehicleBrand:        workOrder.Vehicle.Brand,
            VehicleModel:        workOrder.Vehicle.Model,
            VehicleYear:         workOrder.Vehicle.Year,
            CustomerName:        customerName,
            TotalAmount:         workOrder.TotalAmount,
            Services:            services,
            Parts:               parts,
            ExpiresAt:           approvalToken.ExpiresAt,
            IsExpired:           !approvalToken.IsValid());
    }
}
