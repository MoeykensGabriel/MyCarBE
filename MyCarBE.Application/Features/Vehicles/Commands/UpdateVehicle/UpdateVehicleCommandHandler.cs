using MapsterMapper;
using MediatR;
using MyCarBE.Application.Common.Exceptions;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Application.Common.Validation;
using MyCarBE.Application.Features.Vehicles.DTOs;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, VehicleDto>
{
    private readonly IVehicleRepository  _vehicleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public UpdateVehicleCommandHandler(
        IVehicleRepository  vehicleRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork         unitOfWork,
        IMapper             mapper)
    {
        _vehicleRepository  = vehicleRepository;
        _customerRepository = customerRepository;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
    }

    public async Task<VehicleDto> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Vehicle), request.Id);

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
        if (request.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Domain.Entities.Customer), request.CustomerId.Value);

            if (customer.FleetId.HasValue)
                throw new BadRequestException(
                    "Este cliente es contacto de una flota. No puede tener vehículos individuales asociados. " +
                    "Si necesita registrar un vehículo personal, debe crear otra cuenta sin flota.");
        }

        // Uniqueness checks excluding self
        if (await _vehicleRepository.LicensePlateExistsAsync(licensePlate, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Vehicle), nameof(Domain.Entities.Vehicle.LicensePlate), licensePlate);

        if (vin is not null && await _vehicleRepository.VINExistsAsync(vin, request.Id, cancellationToken))
            throw new ConflictException(nameof(Domain.Entities.Vehicle), nameof(Domain.Entities.Vehicle.VIN), vin);

        vehicle.LicensePlate                     = licensePlate;
        vehicle.Brand                            = request.Brand;
        vehicle.Model                            = request.Model;
        vehicle.Year                             = request.Year;
        vehicle.VIN                              = vin;
        vehicle.EngineNumber                     = engineNumber;
        vehicle.FuelType                         = request.FuelType;
        vehicle.VehicleBodyType                  = request.VehicleBodyType;
        vehicle.VehicleUseType                   = request.VehicleUseType;
        vehicle.Color                            = request.Color;
        vehicle.CurrentMileage                   = request.CurrentMileage;
        vehicle.RegistrationHolderFirstName      = request.RegistrationHolderFirstName;
        vehicle.RegistrationHolderLastName       = request.RegistrationHolderLastName;
        vehicle.RegistrationHolderDocumentType   = request.RegistrationHolderDocumentType;
        vehicle.RegistrationHolderDocumentNumber = holderDocumentNumber;
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
