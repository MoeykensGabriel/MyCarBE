using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleTires.Commands.ReplaceTire;

public class ReplaceTireCommandHandler
    : IRequestHandler<ReplaceTireCommand, VehicleTireDto>
{
    private readonly IVehicleTireRepository _tireRepository;
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IMapper                _mapper;

    public ReplaceTireCommandHandler(
        IVehicleTireRepository tireRepository,
        IVehicleRepository     vehicleRepository,
        ICurrentUserService    currentUser,
        IUnitOfWork            unitOfWork,
        IMapper                mapper)
    {
        _tireRepository    = tireRepository;
        _vehicleRepository = vehicleRepository;
        _currentUser       = currentUser;
        _unitOfWork        = unitOfWork;
        _mapper            = mapper;
    }

    public async Task<VehicleTireDto> Handle(ReplaceTireCommand request, CancellationToken cancellationToken)
    {
        var current = await _tireRepository.GetByIdAsync(request.CurrentTireId, cancellationToken)
            ?? throw new NotFoundException(nameof(VehicleTire), request.CurrentTireId);

        await VehicleOwnershipGuard.EnsureAccessAsync(
            current.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        if (!current.IsActive)
            throw new BadRequestException("La cubierta ya fue reemplazada anteriormente.");

        if (request.ReplacedAtKm < current.InstalledAtKm)
            throw new BadRequestException(
                "Los km de reemplazo no pueden ser menores a los km de instalación de la cubierta actual.");

        // 1) Marcar la actual como reemplazada (queda en historial).
        current.IsActive     = false;
        current.ReplacedOn   = request.ReplacedOn;
        current.ReplacedAtKm = request.ReplacedAtKm;
        _tireRepository.Update(current);

        // 2) Crear la nueva activa en la misma posición.
        var newTire = new VehicleTire
        {
            Id                  = Guid.NewGuid(),
            VehicleId           = current.VehicleId,
            Position            = current.Position,
            Brand               = request.NewBrand.Trim(),
            Model               = request.NewModel.Trim(),
            SizeSpec            = request.NewSizeSpec.Trim(),
            InstalledOn         = request.ReplacedOn,
            InstalledAtKm       = request.ReplacedAtKm,
            InitialTreadDepthMm = request.NewInitialTreadDepthMm,
            ExpectedLifeKm      = request.NewExpectedLifeKm,
            IsActive            = true,
        };
        await _tireRepository.AddAsync(newTire, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return VehicleTireDtoFactory.Build(newTire, _mapper, currentVehicleMileage: request.ReplacedAtKm);
    }
}
