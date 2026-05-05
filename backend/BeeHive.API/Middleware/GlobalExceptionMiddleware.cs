using System.Net;
using System.Text.Json;
using BeeHive.Application.Common.Exceptions;
using ValidationException = BeeHive.Application.Common.Exceptions.ValidationException;

namespace BeeHive.API.Middleware;

/// <summary>
/// Centralized exception handling middleware.
/// Catches known application exceptions and maps them to appropriate HTTP responses.
/// Unknown exceptions return 500 to avoid leaking implementation details.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, errors) = exception switch
        {
            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                new Dictionary<string, string[]> { ["detail"] = [nfe.Message] }
            ),
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation Failed",
                ve.Errors
            ),
            BusinessRuleException bre => (
                HttpStatusCode.UnprocessableEntity,
                "Business Rule Violation",
                new Dictionary<string, string[]> { ["detail"] = [bre.Message] }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                new Dictionary<string, string[]>
                {
                    ["detail"] = [$"{exception.GetType().Name}: {exception.Message}"],
                    ["innerException"] = [exception.InnerException?.Message ?? "none"],
                    ["innerInnerException"] = [exception.InnerException?.InnerException?.Message ?? "none"]
                }
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            type = $"https://httpstatuses.com/{(int)statusCode}",
            title,
            status = (int)statusCode,
            errors
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

/// <summary>Extension method for clean middleware registration in Program.cs.</summary>
public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionMiddleware>();
}
