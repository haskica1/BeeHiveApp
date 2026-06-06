# Architecture

## System Overview

```
Browser (React PWA)
    │  HTTPS / JSON
    ▼
BeeHive.API          ← Controllers, Middleware, JWT auth, ICurrentUser
    │
BeeHive.Application  ← Services, DTOs, Validators, AutoMapper, interfaces, IAccessGuard
    │
BeeHive.Domain       ← Entities, Enums (pure — no dependencies)
    ▲
    ├── BeeHive.Entity         ← EF Core DbContext, Repositories, UoW, Migrations (persistence)
    └── BeeHive.Infrastructure ← External services (email)
    │
PostgreSQL
```

## Dependency Rules

```
API → Application → Domain              (allowed)
Entity → Application, Domain             (allowed — implements Application interfaces)
Infrastructure → Application             (allowed — implements Application interfaces)
API → Entity, Infrastructure             (via DI only, never direct)
Domain → anything else                   (FORBIDDEN)
Application → Entity / Infrastructure     (FORBIDDEN — depends only on its own interfaces)
```

## Backend Layers

### BeeHive.Domain
- `BaseEntity`: `Id (Guid)`, `CreatedAt`, `UpdatedAt`
- All entities inherit `BaseEntity`
- Contains enums, no logic beyond simple property rules
- **No dependencies on other layers**

### BeeHive.Entity (persistence)
- `BeeHiveDbContext` — EF Core (PostgreSQL/Npgsql), auto-stamps `UpdatedAt` on `SaveChangesAsync`
- `Repository<T>` — generic base with CRUD + predicate search; one concrete repo per aggregate (one file each)
- One `IEntityTypeConfiguration<T>` per entity in `Configurations/`
- `UnitOfWork` — single `SaveChangesAsync`, lazy-initialized repos
- Owns EF Core Migrations; database auto-migrates and seeds on startup
- Registered via `AddEntity(configuration)`

### BeeHive.Infrastructure (external services)
- `EmailService` (MailKit/SMTP) and other outbound integrations
- Registered via `AddInfrastructure()`

### BeeHive.Application
- One folder per domain feature: `Apiaries/`, `Beehives/`, `Inspections/`, `Diets/`, `Todos/`, `Auth/`, `Admin/`, `Weather/`, …
- One type per file: `IXxxService.cs`, `XxxService.cs`, `DTOs/<OneDtoPerFile>.cs`, `Validators/<OneValidatorPerFile>.cs`, `XxxMappingProfile.cs`
- `Common/Interfaces/` holds repository + `IUnitOfWork` + `ICurrentUser` interfaces (interfaces live here, **not** in Domain)
- `Common/Security/` holds `Roles` and `IAccessGuard`/`AccessGuard` — the single source of truth for tenant/resource authorization
- `Common/Exceptions/` — `NotFoundException`, `BusinessRuleException`, `ValidationException`, `ForbiddenAccessException` (one per file)
- Services receive `IUnitOfWork`, `ICurrentUser`, and `IAccessGuard` via constructor injection

### BeeHive.API
- Controllers: thin — validate input → call service → return HTTP result (no authorization logic; that lives in the service layer via `IAccessGuard`)
- `CurrentUser` implements `ICurrentUser` from JWT claims via `IHttpContextAccessor`
- `GlobalExceptionMiddleware` — maps exceptions to Problem Details (RFC 7807); `ForbiddenAccessException` → 403
- JWT Bearer auth — all endpoints require `[Authorize]` except `POST /api/auth/login` and the public QR scan lookup
- CORS: configured via `AllowedOrigins`

## Frontend Architecture

```
src/
├── core/
│   ├── context/     ← AuthContext (user, login, logout)
│   ├── models/      ← TypeScript interfaces mirroring backend DTOs
│   └── services/    ← Axios calls + React Query hooks
├── features/        ← One folder per domain (pages live here)
└── shared/
    └── components/  ← Layout, ProtectedRoute, AdminRoute, reusable UI
```

### Data Flow (Frontend)

```
Component
  → React Query hook (queries.ts)
    → Service function (xxxService.ts)
      → apiClient (axios + interceptors)
        → Backend API
```

### Auth Flow

1. `LoginPage` calls `authService.login()` → stores token + user in localStorage
2. `AuthContext` reads localStorage on mount, exposes `user` and `isAuthenticated`
3. `apiClient` request interceptor attaches `Bearer <token>` to every request
4. `apiClient` response interceptor catches 401 → calls `authService.logout()` → redirects to `/login`
5. `ProtectedRoute` blocks unauthenticated users; `AdminRoute` blocks non-SystemAdmin

## Domain Hierarchy

```
Organization
  └── Users (many)
  └── Apiaries (many)
        └── Beehives (many)
              ├── Inspections (many)
              ├── Diets (many)
              │     └── FeedingEntries (many)
              └── Todos (many, optional — can also belong to Apiary)
```

## Key Configuration

| Setting | Location | Value |
|---|---|---|
| DB connection | `appsettings.json` | PostgreSQL (Npgsql) |
| JWT expiry | `appsettings.json` | 480 min (8h) |
| JWT claims | `AuthService` | userId, email, role, organizationId |
| Dev API proxy | `vite.config.ts` | `/api` → `https://localhost:62647` |
| PWA theme | `vite.config.ts` | Amber `#d97706` |
