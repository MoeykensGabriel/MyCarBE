using Microsoft.AspNetCore.Identity;

namespace MyCarBE.Data.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
