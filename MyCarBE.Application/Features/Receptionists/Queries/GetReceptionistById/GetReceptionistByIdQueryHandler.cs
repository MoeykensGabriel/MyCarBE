using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Receptionists.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Receptionists.Queries.GetReceptionistById;

public class GetReceptionistByIdQueryHandler : IRequestHandler<GetReceptionistByIdQuery, ReceptionistDto>
{
    private readonly IReceptionistRepository _repository;
    private readonly IMapper                 _mapper;

    public GetReceptionistByIdQueryHandler(IReceptionistRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<ReceptionistDto> Handle(GetReceptionistByIdQuery request, CancellationToken cancellationToken)
    {
        var receptionist = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Receptionist), request.Id);

        return _mapper.Map<ReceptionistDto>(receptionist);
    }
}
