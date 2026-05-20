using MediatR;
using MyCarBE.Application.Common.Models;
using MyCarBE.Application.Features.Customers.DTOs;

namespace MyCarBE.Application.Features.Customers.Queries.GetAllCustomers;

/// <param name="SearchTerm">Optional. Filters by first name, last name, document number or phone.</param>
public record GetAllCustomersQuery(
    string? SearchTerm = null,
    int     Page       = 1,
    int     PageSize   = 20
) : IRequest<PagedResult<CustomerDto>>;
