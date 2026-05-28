using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.InspectionReports.Queries.GetInspectionReportById;

public class GetInspectionReportByIdQueryHandler : IRequestHandler<GetInspectionReportByIdQuery, InspectionReportDto>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IMapper                     _mapper;

    public GetInspectionReportByIdQueryHandler(IInspectionReportRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<InspectionReportDto> Handle(GetInspectionReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(InspectionReport), request.Id);

        // Reload con includes para que el mapping resuelva AreaName/MechanicFullName/Photos
        var full = await _repository.GetByWorkOrderAndAreaAsync(report.WorkOrderId, report.AreaId, cancellationToken);
        return _mapper.Map<InspectionReportDto>(full!);
    }
}
