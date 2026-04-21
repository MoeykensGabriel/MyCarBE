using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork        _unitOfWork;

    public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        // Uniqueness checks
        if (await _vehicleRepository.LicensePlateExistsAsync(request.LicensePlate, cancellationToken))
            throw new ConflictException(nameof(Vehicle), nameof(Vehicle.LicensePlate), request.LicensePlate);

        if (request.VIN is not null && await _vehicleRepository.VINExistsAsync(request.VIN, cancellationToken))
            throw new ConflictException(nameof(Vehicle), nameof(Vehicle.VIN), request.VIN);

        var vehicle = new Vehicle
        {
            LicensePlate                      = request.LicensePlate,
            Brand                             = request.Brand,
            Model                             = request.Model,
            Year                              = request.Year,
            VIN                               = request.VIN,
            EngineNumber                      = request.EngineNumber,
            FuelType                          = request.FuelType,
            VehicleBodyType                   = request.VehicleBodyType,
            VehicleUseType                    = request.VehicleUseType,
            Color                             = request.Color,
            CurrentMileage                    = request.CurrentMileage,
            RegistrationHolderFirstName       = request.RegistrationHolderFirstName,
            RegistrationHolderLastName        = request.RegistrationHolderLastName,
            RegistrationHolderDocumentType    = request.RegistrationHolderDocumentType,
            RegistrationHolderDocumentNumber  = request.RegistrationHolderDocumentNumber,
            RegistrationCertificateNumber     = request.RegistrationCertificateNumber,
            CustomerId                        = request.CustomerId,
            FleetId                           = request.FleetId,
        };

        // Domain-level XOR guard (second safety net after validator)
        vehicle.ValidateOwnership();

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vehicle.Id;
    }
}
