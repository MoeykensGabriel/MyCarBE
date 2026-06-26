using MediatR;
using MyCarBE.Application.Features.Sales.DTOs;

namespace MyCarBE.Application.Features.Sales.Queries.GetSaleById;

public record GetSaleByIdQuery(Guid Id) : IRequest<SaleDto>;
