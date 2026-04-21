using BeeHive.Application.Common.Behaviors;
using BeeHive.Application.Common.Mappings;
using BeeHive.Application.Common.Services;
using BeeHive.Application.Features.Apiaries;
using BeeHive.Application.Features.Auth;
using BeeHive.Application.Features.Beehives;
using BeeHive.Application.Features.Diets;
using BeeHive.Application.Features.Inspections;
using BeeHive.Application.Features.Todos;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BeeHive.Application;

/// <summary>
/// Extension method to register all Application-layer services in the DI container.
/// Called from the API's Program.cs.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper — scans this assembly for all Profile subclasses
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // FluentValidation — registers all validators in this assembly
        services.AddValidatorsFromAssemblyContaining<CreateApiaryValidator>();

        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApiaryService, ApiaryService>();
        services.AddScoped<IBeehiveService, BeehiveService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IDietService, DietService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();

        return services;
    }
}
