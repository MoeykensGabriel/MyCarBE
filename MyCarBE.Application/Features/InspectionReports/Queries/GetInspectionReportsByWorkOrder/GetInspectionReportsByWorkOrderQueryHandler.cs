using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.InspectionReports.DTOs;

namespace MyCarBE.Application.Features.InspectionReports.Queries.GetInspectionReportsByWorkOrder;

public class GetInspectionReportsByWorkOrderQueryHandler : IRequestHandler<GetInspectionReportsByWorkOrderQuery, IReadOnlyList<InspectionReportDto>>
{
    private readonly IInspectionReportRepository _repository;
    private readonly IMapper                     _mapper;

    public GetInspectionReportsByWorkOrderQueryHandler(IInspectionReportRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<InspectionReportDto>> Handle(GetInspectionReportsByWorkOrderQuery request, CancellationToken cancellationToken)
    {
        var reports = await _repository.GetByWorkOrderAsync(request.WorkOrderId, cancellationToken);
        return reports.Select(r => _mapper.Map<InspectionReportDto>(r)).ToList();
    }
}
