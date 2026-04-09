using BeeHive.API.Middleware;
using BeeHive.Application;
using BeeHive.Infrastructure;
using BeeHive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "BeeHive API",
        Version = "v1",
        Description = "REST API for managing beekeeping operations — apiaries, beehives, and inspections."
    });

    // Include XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// Register Application and Infrastructure layers via extension methods
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS — allow the Angular/React frontend running on localhost during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",  // Angular default
                "http://localhost:5173"   // Vite/React default
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────

// Global exception handler must be first in the pipeline
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeeHive API v1"));
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.UseAuthorization();
app.MapControllers();

// ── Database Initialisation ───────────────────────────────────────────────────
// Automatically apply pending migrations on startup (development convenience).
// For production, use a dedicated migration step in your CI/CD pipeline instead.

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BeeHiveDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
