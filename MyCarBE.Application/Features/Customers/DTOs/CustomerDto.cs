using MyCarBE.Domain.Enums;

namespace MyCarBE.Application.Features.Customers.DTOs;

public record CustomerDto(
    Guid            Id,
    string          FirstName,
    string          LastName,
    DocumentType    DocumentType,
    string          DocumentNumber,
    string          Phone,
    string?         Email,
    Guid?           ApplicationUserId,
    Guid?           FleetId,
    DateTime        CreatedAt,
    DateTime        UpdatedAt
);
