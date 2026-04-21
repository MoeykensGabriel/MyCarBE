using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.CatalogServices.DTOs;

namespace MyCarBE.Application.Features.CatalogServices.Queries.GetAllCatalogServices;

public class GetAllCatalogServicesQueryHandler : IRequestHandler<GetAllCatalogServicesQuery, IReadOnlyList<CatalogServiceDto>>
{
    private readonly ICatalogServiceRepository _repository;
    private readonly IMapper                   _mapper;

    public GetAllCatalogServicesQueryHandler(ICatalogServiceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    public async Task<IReadOnlyList<CatalogServiceDto>> Handle(GetAllCatalogServicesQuery request, CancellationToken cancellationToken)
    {
        var services = request.IncludeInactive
            ? await _repository.GetAllWithInactiveAsync(cancellationToken)
            : await _repository.GetActiveAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<CatalogServiceDto>>(services);
    }
}
