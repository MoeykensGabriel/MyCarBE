using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository  _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork         _unitOfWork;

    public CreateVehicleCommandHandler(
        IVehicleRepository  vehicleRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork         unitOfWork)
    {
        _vehicleRepository  = vehicleRepository;
        _customerRepository = customerRepository;
        _unitOfWork         = unitOfWork;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        // Normalizar patente: sin espacios, mayúsculas
        var licensePlate = request.LicensePlate.Replace(" ", "").ToUpperInvariant();

        // Normalizar strings opcionales: "" → null
        var vin          = string.IsNullOrWhiteSpace(request.VIN)          ? null : request.VIN.Trim().ToUpperInvariant();
        var engineNumber = string.IsNullOrWhiteSpace(request.EngineNumber) ? null : request.EngineNumber.Trim();

        // Normalizar el documento del titular registral según su tipo
        var holderDocumentNumber = request.RegistrationHolderDocumentType switch
        {
            DocumentType.DNI                       => ArgentinaIdentifiers.NormalizeDni(request.RegistrationHolderDocumentNumber),
            DocumentType.CUIT or DocumentType.CUIL => ArgentinaIdentifiers.NormalizeCuit(request.RegistrationHolderDocumentNumber),
            DocumentType.Passport                  => ArgentinaIdentifiers.NormalizePassport(request.RegistrationHolderDocumentNumber),
            _                                      => request.RegistrationHolderDocumentNumber.Trim(),
        };

        // Regla de negocio: un contacto de flota no puede tener vehículos individuales.
        // Si quiere registrar su auto personal, debe crear otra cuenta sin fleetId.
        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId.Value);

            if (customer.FleetId.HasValue)
                throw new BadRequestException(
                    "Este cliente es contacto de una flota. No puede tener vehículos individuales asociados. " +
                    "Si necesita registrar un vehículo personal, debe crear otra cuenta sin flota.");
        }

        // Uniqueness checks
        if (await _vehicleRepository.LicensePlateExistsAsync(licensePlate, cancellationToken))
            throw new ConflictException(nameof(Vehicle), nameof(Vehicle.LicensePlate), licensePlate);

        if (vin is not null && await _vehicleRepository.VINExistsAsync(vin, cancellationToken))
            throw new ConflictException(nameof(Vehicle), nameof(Vehicle.VIN), vin);

        var vehicle = new Vehicle
        {
            LicensePlate                      = licensePlate,
            Brand                             = request.Brand,
            Model                             = request.Model,
            Year                              = request.Year,
            VIN                               = vin,
            EngineNumber                      = engineNumber,
            FuelType                          = request.FuelType,
            VehicleBodyType                   = request.VehicleBodyType,
            VehicleUseType                    = request.VehicleUseType,
            Color                             = request.Color,
            CurrentMileage                    = request.CurrentMileage,
            RegistrationHolderFirstName       = request.RegistrationHolderFirstName,
            RegistrationHolderLastName        = request.RegistrationHolderLastName,
            RegistrationHolderDocumentType    = request.RegistrationHolderDocumentType,
            RegistrationHolderDocumentNumber  = holderDocumentNumber,
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
