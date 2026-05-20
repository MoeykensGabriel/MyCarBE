using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Settings.DTOs;

namespace MyCarBE.Application.Features.Settings.Commands.UpdateWorkshopSettings;

public class UpdateWorkshopSettingsCommandHandler
    : IRequestHandler<UpdateWorkshopSettingsCommand, WorkshopSettingsDto>
{
    private readonly IWorkshopSettingsRepository _repository;
    private readonly IUnitOfWork                 _unitOfWork;

    public UpdateWorkshopSettingsCommandHandler(
        IWorkshopSettingsRepository repository,
        IUnitOfWork                 unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkshopSettingsDto> Handle(
        UpdateWorkshopSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var settings = await _repository.GetAsync(cancellationToken);

        settings.PhysicalCapacity = request.PhysicalCapacity;

        _repository.Update(settings);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkshopSettingsDto(settings.PhysicalCapacity);
    }
}
