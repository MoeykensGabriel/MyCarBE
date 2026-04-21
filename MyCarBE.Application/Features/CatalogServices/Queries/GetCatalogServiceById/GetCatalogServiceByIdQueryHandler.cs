using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.CatalogServices.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.CatalogServices.Queries.GetCatalogServiceById;

public class GetCatalogServiceByIdQueryHandler : IRequestHandler<GetCatalogServiceByIdQuery, CatalogServiceDto>
{
    private readonly ICatalogServiceRepository _repository;
    private readonly IMapper                   _mapper;

    public GetCatalogServiceByIdQueryHandler(ICatalogServiceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<CatalogServiceDto> Handle(GetCatalogServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(CatalogService), request.Id);

        return _mapper.Map<CatalogServiceDto>(service);
    }
}
