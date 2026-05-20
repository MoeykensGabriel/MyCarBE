namespace MyCarBE.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public string    Token      { get; set; } = string.Empty;
    public string    Role       { get; set; } = string.Empty;
    public string    Email      { get; set; } = string.Empty;
    public string    FullName   { get; set; } = string.Empty;
    public DateTime  ExpiresAt  { get; set; }
    public Guid      UserId     { get; set; }
    public Guid?     CustomerId { get; set; }
    public Guid?     FleetId    { get; set; }
    public Guid?     MechanicId { get; set; }
}
