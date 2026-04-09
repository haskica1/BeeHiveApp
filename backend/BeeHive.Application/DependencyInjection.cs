using BeeHive.Application.Common.Behaviors;
using BeeHive.Application.Common.Mappings;
using BeeHive.Application.Features.Apiaries;
using BeeHive.Application.Features.Beehives;
using BeeHive.Application.Features.Inspections;
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
        services.AddScoped<IApiaryService, ApiaryService>();
        services.AddScoped<IBeehiveService, BeehiveService>();
        services.AddScoped<IInspectionService, InspectionService>();

        return services;
    }
}
