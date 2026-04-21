using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCarBE.Data.Identity;

namespace MyCarBE.Data.Services;

/// <summary>
/// Crea el usuario Admin inicial si no existe.
/// Se ejecuta una vez al iniciar la aplicación.
/// Credenciales configurables en appsettings — cambiarlas en producción.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        using var scope       = serviceProvider.CreateScope();
        var userManager       = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger            = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationUser>>();

        const string adminEmail    = "admin@mycar.com";
        const string adminPassword = "Admin@1234";

        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing is not null)
        {
            logger.LogInformation("Admin user already exists — skipping seed.");
            return;
        }

        var admin = new ApplicationUser
        {
            Id        = Guid.NewGuid(),
            UserName  = adminEmail,
            Email     = adminEmail,
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Failed to seed admin user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(admin, "Admin");
        logger.LogInformation("Admin user seeded → {Email} / {Password}", adminEmail, adminPassword);
    }
}
