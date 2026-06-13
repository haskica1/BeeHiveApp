using System.Text;
using System.Threading.RateLimiting;
using BeeHive.API.Middleware;
using BeeHive.Application;
using BeeHive.Application.Features.Inspections;
using BeeHive.Application.Features.Weather;
using BeeHive.Entity;
using BeeHive.Entity.Seed;
using BeeHive.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Services ───────────────────────────��─────────────────────────────────────

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new BeeHive.API.Middleware.UtcDateTimeJsonConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "BeeHive API",
        Version = "v1",
        Description = "REST API for managing beekeeping operations — apiaries, beehives, and inspections."
    });

    // JWT auth button in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// Register Application, persistence (Entity), and Infrastructure layers via extension methods
builder.Services.AddApplication();
builder.Services.AddEntity(builder.Configuration);
builder.Services.AddInfrastructure();

// Current-user abstraction — resolves the authenticated caller from JWT claims for the
// Application layer's authorization (see ICurrentUser / IAccessGuard).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BeeHive.Application.Common.Interfaces.ICurrentUser, BeeHive.API.Security.CurrentUser>();

// Weather service — typed HttpClient targeting Open-Meteo (free, no API key needed)
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Voice parsing service — calls Google Gemini API; key configured via Gemini:ApiKey
builder.Services.AddHttpClient<IVoiceParsingService, VoiceParsingService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// CORS — origins are configured via AllowedOrigins in appsettings.json.
// On Render, override with the env var:  AllowedOrigins=https://your-app.vercel.app
var allowedOrigins = (builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Rate limiting — throttle login attempts per client IP to slow credential-stuffing/brute force.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));

    // Token refresh is a legitimate periodic operation — keep it separate from (and more
    // generous than) login so a burst of refreshes can't lock out sign-ins, and vice versa.
    options.AddPolicy("auth-refresh", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            }));
});

// Health check — liveness probe at /health for the deployment platform.
builder.Services.AddHealthChecks();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────��──────────────────────

// Global exception handler must be first in the pipeline
app.UseGlobalExceptionHandling();

// Swagger available in all environments so the deployed API can be explored
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeeHive API v1"));

// HTTPS redirect only in local dev — Render (and most cloud hosts) terminate
// TLS at the proxy level; the container itself only serves plain HTTP.
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ── Database Initialisation ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BeeHiveDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseInitializer.SeedUsersAsync(db);
}

app.Run();
