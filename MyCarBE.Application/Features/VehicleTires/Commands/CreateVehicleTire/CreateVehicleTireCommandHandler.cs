using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.VehicleDocuments; // reusa VehicleOwnershipGuard (internal, misma assembly)
using MyCarBE.Application.Features.VehicleTires.DTOs;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.VehicleTires.Commands.CreateVehicleTire;

public class CreateVehicleTireCommandHandler
    : IRequestHandler<CreateVehicleTireCommand, VehicleTireDto>
{
    private readonly IVehicleTireRepository _tireRepository;
    private readonly IVehicleRepository     _vehicleRepository;
    private readonly ICurrentUserService    _currentUser;
    private readonly IUnitOfWork            _unitOfWork;
    private readonly IMapper                _mapper;

    public CreateVehicleTireCommandHandler(
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

    public async Task<VehicleTireDto> Handle(CreateVehicleTireCommand request, CancellationToken cancellationToken)
    {
        // Crear cubierta es operación de taller: solo Admin/Mechanic/Receptionist deberían
        // poder hacerlo. Eso lo hace cumplir el endpoint (Authorize). Acá igual chequeamos
        // que el vehículo exista — el guard tira NotFound si no.
        await VehicleOwnershipGuard.EnsureAccessAsync(
            request.VehicleId, _vehicleRepository, _currentUser, cancellationToken);

        // Si ya hay una activa en esa posición, la marcamos como reemplazada por esta nueva.
        // Esto evita violar el invariante "1 cubierta activa por (vehicleId, position)".
        var existing = await _tireRepository.GetActiveByPositionAsync(
            request.VehicleId, request.Position, cancellationToken);
        if (existing is not null)
        {
            existing.IsActive     = false;
            existing.ReplacedOn   = request.InstalledOn;
            existing.ReplacedAtKm = request.InstalledAtKm;
            _tireRepository.Update(existing);
        }

        var tire = new VehicleTire
        {
            Id                  = Guid.NewGuid(),
            VehicleId           = request.VehicleId,
            Position            = request.Position,
            Brand               = request.Brand.Trim(),
            Model               = request.Model.Trim(),
            SizeSpec            = request.SizeSpec.Trim(),
            InstalledOn         = request.InstalledOn,
            InstalledAtKm       = request.InstalledAtKm,
            InitialTreadDepthMm = request.InitialTreadDepthMm,
            ExpectedLifeKm      = request.ExpectedLifeKm,
            IsActive            = true,
        };

        await _tireRepository.AddAsync(tire, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return VehicleTireDtoFactory.Build(tire, _mapper);
    }
}
