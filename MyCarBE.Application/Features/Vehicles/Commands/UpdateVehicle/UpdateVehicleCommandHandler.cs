using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Features.Vehicles.DTOs;

namespace MyCarBE.Application.Features.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork        _unitOfWork;
    private readonly IMapper            _mapper;

    public UpdateVehicleCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork        = unitOfWork;
        _mapper            = mapper;
    }

    public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Vehicle), request.Id);

        // Uniqueness checks excluding self
        if (await _vehicleRepository.LicensePlateExistsAsync(request.LicensePlate, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Vehicle), nameof(Domain.Entities.Vehicle.LicensePlate), request.LicensePlate);

        if (request.VIN is not null && await _vehicleRepository.VINExistsAsync(request.VIN, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Vehicle), nameof(Domain.Entities.Vehicle.VIN), request.VIN);

        vehicle.LicensePlate                     = request.LicensePlate;
        vehicle.Brand                            = request.Brand;
        vehicle.Model                            = request.Model;
        vehicle.Year                             = request.Year;
        vehicle.VIN                              = request.VIN;
        vehicle.EngineNumber                     = request.EngineNumber;
        vehicle.FuelType                         = request.FuelType;
        vehicle.VehicleBodyType                  = request.VehicleBodyType;
        vehicle.VehicleUseType                   = request.VehicleUseType;
        vehicle.Color                            = request.Color;
        vehicle.CurrentMileage                   = request.CurrentMileage;
        vehicle.RegistrationHolderFirstName      = request.RegistrationHolderFirstName;
        vehicle.RegistrationHolderLastName       = request.RegistrationHolderLastName;
        vehicle.RegistrationHolderDocumentType   = request.RegistrationHolderDocumentType;
        vehicle.RegistrationHolderDocumentNumber = request.RegistrationHolderDocumentNumber;
        vehicle.RegistrationCertificateNumber    = request.RegistrationCertificateNumber;
        vehicle.CustomerId                       = request.CustomerId;
        vehicle.FleetId                          = request.FleetId;

        // Domain-level XOR guard
        vehicle.ValidateOwnership();

        _vehicleRepository.Update(vehicle);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VehicleDto>(vehicle);
    }
}
