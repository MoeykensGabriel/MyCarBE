using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Areas.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Areas.Commands.UpdateArea;

public class UpdateAreaCommandHandler : IRequestHandler<UpdateAreaCommand, AreaDto>
{
    private readonly IAreaRepository _repository;
    private readonly IUnitOfWork     _unitOfWork;
    private readonly IMapper         _mapper;

    public UpdateAreaCommandHandler(IAreaRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<AreaDto> Handle(UpdateAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Area), request.Id);

        var name = request.Name.Trim();

        if (!string.Equals(area.Name, name, StringComparison.OrdinalIgnoreCase) &&
            await _repository.NameExistsAsync(name, request.Id, cancellationToken))
        {
            throw new ConflictException(nameof(Area), nameof(request.Name), name);
        }

        area.Name     = name;
        area.IsActive = request.IsActive;

        _repository.Update(area);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AreaDto>(area);
    }
}
