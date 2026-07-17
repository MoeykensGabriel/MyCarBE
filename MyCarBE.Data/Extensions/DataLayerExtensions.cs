using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyCarBE.Application.Common.Interfaces;
using MyCarBE.Application.Common.Interfaces.Repositories;
using MyCarBE.Data.Context;
using MyCarBE.Data.Identity;
using MyCarBE.Data.Repositories;
using MyCarBE.Data.Services;

namespace MyCarBE.Data.Extensions;

public static class DataLayerExtensions
{
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL + EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                ResolveConnectionString(configuration),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            )
        );

        // ASP.NET Identity
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength         = 8;
            options.Password.RequireDigit           = true;
            options.Password.RequireUppercase       = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail         = true;

            // Lockout (anti fuerza-bruta): tras N intentos fallidos la cuenta
            // queda bloqueada temporalmente. El conteo lo lleva IdentityService.LoginAsync
            // (AccessFailedAsync / ResetAccessFailedCountAsync) — ver P0 en SPEC.
            options.Lockout.AllowedForNewUsers      = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        // Unit of Work — AppDbContext ya es Scoped, lo exponemos como IUnitOfWork
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositorios
        services.AddScoped<ICustomerRepository,      CustomerRepository>();
        services.AddScoped<IFleetRepository,         FleetRepository>();
        services.AddScoped<IVehicleRepository,       VehicleRepository>();
        services.AddScoped<IWorkOrderRepository,     WorkOrderRepository>();
        services.AddScoped<ICatalogServiceRepository, CatalogServiceRepository>();
        services.AddScoped<IWorkOrderApprovalTokenRepository, WorkOrderApprovalTokenRepository>();
        services.AddScoped<IDashboardRepository,             DashboardRepository>();
        services.AddScoped<IMechanicRepository,              MechanicRepository>();
        services.AddScoped<IReceptionistRepository,          ReceptionistRepository>();
        services.AddScoped<IAreaRepository,                  AreaRepository>();
        services.AddScoped<IInspectionReportRepository,      InspectionReportRepository>();
        services.AddScoped<IWorkshopSettingsRepository,      WorkshopSettingsRepository>();
        services.AddScoped<IPartsStockRequestRepository,     PartsStockRequestRepository>();
        services.AddScoped<IVehicleDocumentRepository,       VehicleDocumentRepository>();
        services.AddScoped<IVehicleTripRepository,           VehicleTripRepository>();
        services.AddScoped<IVehicleTireRepository,           VehicleTireRepository>();
        services.AddScoped<IVehicleBatteryRepository,        VehicleBatteryRepository>();
        services.AddScoped<IVehicleOilServiceRepository,     VehicleOilServiceRepository>();
        services.AddScoped<IVehicleMileageReadingRepository, VehicleMileageReadingRepository>();
        services.AddScoped<IMaintenanceAlertRepository,       MaintenanceAlertRepository>();
        services.AddScoped<ISaleRepository,                   SaleRepository>();

        // Identity Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();

        // Sistema de Stock externo (GestionPGB).
        // Si StockSystem:BaseUrl está configurado → HttpStockService (HTTP real a GestionPGB).
        // Si no → StubStockService (fallback que solo loguea — útil sin el depósito corriendo).
        var stockBaseUrl = configuration["StockSystem:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(stockBaseUrl))
        {
            services.AddHttpClient<IStockService, HttpStockService>(client =>
            {
                client.BaseAddress = new Uri(stockBaseUrl);
                client.Timeout     = TimeSpan.FromSeconds(15);
            });
        }
        else
        {
            services.AddScoped<IStockService, StubStockService>();
        }

        // FluentValidation — registra todos los validators del assembly de Data
        services.AddValidatorsFromAssembly(typeof(DataLayerExtensions).Assembly);

        return services;
    }

    /// <summary>
    /// Resuelve la connection string Postgres desde las variables que puede exponer
    /// Railway (CONNECTION_STRING / DATABASE_URL / DATABASE_PUBLIC_URL) o appsettings
    /// (DefaultConnection) en local. Railway la entrega como URI (postgresql://...),
    /// formato que Npgsql NO parsea: se convierte a key=value con SslMode.Prefer
    /// (negocia SSL — lo usa en el proxy público que lo exige y funciona en la red
    /// interna). Mismo patrón que GestionPGB, ya probado en producción.
    /// </summary>
    private static string? ResolveConnectionString(IConfiguration configuration)
    {
        static string? NonEmpty(string? v) => string.IsNullOrWhiteSpace(v) ? null : v;

        var raw = NonEmpty(Environment.GetEnvironmentVariable("CONNECTION_STRING"))
               ?? NonEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"))
               ?? NonEmpty(Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL"))
               ?? NonEmpty(configuration.GetConnectionString("DefaultConnection"));

        if (string.IsNullOrWhiteSpace(raw))
            return null; // EF tirará su error estándar de connection string faltante

        // Ya viene en formato key=value (local) → sin cambios.
        if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return raw;

        var uri      = new Uri(raw);
        var userInfo = uri.UserInfo.Split(':', 2);

        var csb = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host     = uri.Host,
            Port     = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = uri.AbsolutePath.TrimStart('/'),
            SslMode  = Npgsql.SslMode.Prefer,
        };

        return csb.ConnectionString;
    }
}
