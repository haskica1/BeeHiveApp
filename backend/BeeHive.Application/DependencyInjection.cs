using BeeHive.Application.Common.Security;
using BeeHive.Application.Features.Apiaries.Validators;
using BeeHive.Application.Common.Services;
using BeeHive.Application.Features.Admin;
using BeeHive.Application.Features.Advisor;
using BeeHive.Application.Features.Alerts;
using BeeHive.Application.Features.Apiaries;
using BeeHive.Application.Features.OrgManagement;
using BeeHive.Application.Features.Auth;
using BeeHive.Application.Features.Calendar;
using BeeHive.Application.Features.Notifications;
using BeeHive.Application.Features.Profile;
using BeeHive.Application.Features.Stats;
using BeeHive.Application.Features.Beehives;
using BeeHive.Application.Features.Diets;
using BeeHive.Application.Features.Expenses;
using BeeHive.Application.Features.Harvests;
using BeeHive.Application.Features.Inspections;
using BeeHive.Application.Features.Learning;
using BeeHive.Application.Features.Pastures;
using BeeHive.Application.Features.Queens;
using BeeHive.Application.Features.Todos;
using BeeHive.Application.Features.Treatments;
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
        // AutoMapper — scans this assembly for all per-feature Profile subclasses
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        // FluentValidation — registers all validators in this assembly
        services.AddValidatorsFromAssemblyContaining<CreateApiaryValidator>();

        // Cross-cutting authorization — single source of truth for tenant/resource access
        services.AddScoped<IAccessGuard, AccessGuard>();

        // Subscription-plan enforcement (SPEC-09) — single source of truth for plan gates
        services.AddScoped<IPlanGuard, PlanGuard>();

        // Application services
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IOrgManagementService, OrgManagementService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IApiaryService, ApiaryService>();
        services.AddScoped<IBeehiveService, BeehiveService>();
        services.AddScoped<IInspectionService, InspectionService>();
        services.AddScoped<IInspectionPhotoService, InspectionPhotoService>();
        services.AddScoped<IQueenService, QueenService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IDietService, DietService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IHarvestService, HarvestService>();
        services.AddScoped<ITreatmentService, TreatmentService>();
        services.AddScoped<ILearningTopicService, LearningTopicService>();
        services.AddScoped<IPastureService, PastureService>();
        services.AddScoped<IApiaryMoveService, ApiaryMoveService>();
        services.AddScoped<IAdvisorService, AdvisorService>();
        services.AddScoped<IAlertRuleService, AlertRuleService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();

        return services;
    }
}
