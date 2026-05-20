using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyCarBE.Data.Context;
using MyCarBE.Data.Identity;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Services;

/// <summary>
/// Seeds iniciales al arrancar la aplicación. Idempotente: si los datos ya
/// existen, no hace nada.
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
        }
        else
        {
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

        // ── Workshop settings: garantizamos que exista la fila singleton ──
        await SeedWorkshopSettingsAsync(scope.ServiceProvider);
    }

    private static async Task SeedWorkshopSettingsAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        var logger    = serviceProvider.GetRequiredService<ILogger<WorkshopSettings>>();

        var any = await dbContext.WorkshopSettings.AnyAsync();
        if (any)
        {
            logger.LogInformation("Workshop settings already exist — skipping seed.");
            return;
        }

        dbContext.WorkshopSettings.Add(new WorkshopSettings
        {
            Id               = Guid.NewGuid(),
            PhysicalCapacity = 6,
            CreatedAt        = DateTime.UtcNow,
            UpdatedAt        = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Workshop settings seeded with default capacity = 6.");
    }
}
