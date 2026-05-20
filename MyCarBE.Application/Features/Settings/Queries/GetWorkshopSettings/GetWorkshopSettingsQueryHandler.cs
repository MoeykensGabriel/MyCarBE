using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Settings.DTOs;

namespace MyCarBE.Application.Features.Settings.Queries.GetWorkshopSettings;

public class GetWorkshopSettingsQueryHandler : IRequestHandler<GetWorkshopSettingsQuery, WorkshopSettingsDto>
{
    private readonly IWorkshopSettingsRepository _repository;

    public GetWorkshopSettingsQueryHandler(IWorkshopSettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkshopSettingsDto> Handle(GetWorkshopSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.GetAsync(cancellationToken);
        return new WorkshopSettingsDto(settings.PhysicalCapacity);
    }
}
