using Microsoft.AspNetCore.Identity;

namespace MyCarBE.Data.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
