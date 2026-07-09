using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.WorkOrders.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.WorkOrders.Commands.ReviseQuote;

public class ReviseQuoteCommandHandler : IRequestHandler<ReviseQuoteCommand, WorkOrderDetailDto>
{
    private readonly IWorkOrderRepository              _workOrderRepository;
    private readonly IWorkOrderApprovalTokenRepository _tokenRepository;
    private readonly ICurrentUserService               _currentUser;
    private readonly IUnitOfWork                       _unitOfWork;
    private readonly IMapper                           _mapper;

    public ReviseQuoteCommandHandler(
        IWorkOrderRepository              workOrderRepository,
        IWorkOrderApprovalTokenRepository tokenRepository,
        ICurrentUserService               currentUser,
        IUnitOfWork                       unitOfWork,
        IMapper                           mapper)
    {
        _workOrderRepository = workOrderRepository;
        _tokenRepository     = tokenRepository;
        _currentUser         = currentUser;
        _unitOfWork          = unitOfWork;
        _mapper              = mapper;
    }

    public async Task<WorkOrderDetailDto> Handle(ReviseQuoteCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _workOrderRepository.GetWithFullDetailsAsync(request.WorkOrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(WorkOrder), request.WorkOrderId);

        try
        {
            // Transición + descongelar items + limpiar TTL — todo en el dominio.
            workOrder.ReturnToDiagnosing(_currentUser.UserId, request.Note);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        // El link de aprobación del email viejo deja de funcionar: el presupuesto que el
        // cliente tenía en la mano ya no es el que va a terminar aprobando.
        var activeToken = await _tokenRepository.GetActiveByWorkOrderIdAsync(workOrder.Id, cancellationToken);
        if (activeToken is not null)
            _tokenRepository.Delete(activeToken);

        _workOrderRepository.Update(workOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkOrderDetailDto>(workOrder);
    }
}
