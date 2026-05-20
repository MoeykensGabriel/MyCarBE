namespace MyCarBE.Application.Features.Receptionists.DTOs;

public record ReceptionistDto(
    Guid     Id,
    string   FirstName,
    string   LastName,
    string   Email,
    bool     IsActive,
    Guid     ApplicationUserId,
    DateTime CreatedAt
);
