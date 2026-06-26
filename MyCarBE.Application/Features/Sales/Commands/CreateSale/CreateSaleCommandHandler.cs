using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Sales.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Sales.Commands.CreateSale;

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleDto>
{
    private readonly ISaleRepository         _saleRepository;
    private readonly IReceptionistRepository _receptionistRepository;
    private readonly ICurrentUserService     _currentUser;
    private readonly IUnitOfWork             _unitOfWork;

    public CreateSaleCommandHandler(
        ISaleRepository         saleRepository,
        IReceptionistRepository receptionistRepository,
        ICurrentUserService     currentUser,
        IUnitOfWork             unitOfWork)
    {
        _saleRepository         = saleRepository;
        _receptionistRepository = receptionistRepository;
        _currentUser            = currentUser;
        _unitOfWork             = unitOfWork;
    }

    public async Task<SaleDto> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        // Vendedor = usuario logueado. Snapshot del nombre: ficha de recepcionista si la tiene;
        // si no (ej. el Admin, que no tiene ficha), cae al email.
        var receptionist = await _receptionistRepository
            .GetByApplicationUserIdAsync(_currentUser.UserId, cancellationToken);
        var sellerName = receptionist is not null
            ? $"{receptionist.FirstName} {receptionist.LastName}".Trim()
            : _currentUser.Email;

        var sale = new Sale
        {
            CustomerId   = request.CustomerId,
            FleetId      = request.FleetId,
            SellerUserId = _currentUser.UserId,
            SellerName   = sellerName,
            Items = request.Items.Select(i => new SaleItem
            {
                ProductCode = string.IsNullOrWhiteSpace(i.ProductCode) ? null : i.ProductCode.Trim(),
                Name        = i.Name.Trim(),
                UnitPrice   = i.UnitPrice,
                Quantity    = i.Quantity,
            }).ToList(),
        };
        sale.RecalculateTotalAmount();

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargo con comprador + ítems para armar el DTO (BuyerName se resuelve por join).
        var saved = await _saleRepository.GetByIdWithDetailsAsync(sale.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Sale), sale.Id);

        return SaleDtoFactory.Build(saved);
    }
}
