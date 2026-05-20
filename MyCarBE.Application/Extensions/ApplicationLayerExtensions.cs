using System.Reflection;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyCarBE.Application.Common.Behaviours;

namespace MyCarBE.Application.Extensions;

public static class ApplicationLayerExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR — registra todos los Handlers del assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Pipeline Behaviour: validación automática antes de cada Handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // FluentValidation — registra todos los Validators del assembly
        services.AddValidatorsFromAssembly(assembly);

        // Mapster — escanea todos los IRegister del assembly
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
