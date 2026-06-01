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
                configuration.GetConnectionString("DefaultConnection"),
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
}
