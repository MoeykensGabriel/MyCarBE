using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Areas.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Areas.Commands.CreateArea;

public class CreateAreaCommandHandler : IRequestHandler<CreateAreaCommand, AreaDto>
{
    private readonly IAreaRepository _repository;
    private readonly IUnitOfWork     _unitOfWork;
    private readonly IMapper         _mapper;

    public CreateAreaCommandHandler(IAreaRepository repository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper     = mapper;
    }

    public async Task<AreaDto> Handle(CreateAreaCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await _repository.NameExistsAsync(name, cancellationToken))
            throw new ConflictException(nameof(Area), nameof(request.Name), name);

        var area = new Area
        {
            Id       = Guid.NewGuid(),
            Name     = name,
            IsActive = true,
        };

        await _repository.AddAsync(area, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AreaDto>(area);
    }
}
