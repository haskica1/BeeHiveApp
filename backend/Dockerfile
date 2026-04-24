# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so the restore layer is cached
# (only re-runs when a .csproj changes, not on every code edit)
COPY BeeHive.Domain/BeeHive.Domain.csproj                 BeeHive.Domain/
COPY BeeHive.Application/BeeHive.Application.csproj       BeeHive.Application/
COPY BeeHive.Infrastructure/BeeHive.Infrastructure.csproj BeeHive.Infrastructure/
COPY BeeHive.API/BeeHive.API.csproj                       BeeHive.API/

RUN dotnet restore BeeHive.API/BeeHive.API.csproj

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish BeeHive.API/BeeHive.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injects $PORT at container startup.
# The shell-form entrypoint expands $PORT at runtime (not build time).
# Falls back to 8080 so `docker run` works locally without setting PORT.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet BeeHive.API.dll"]
