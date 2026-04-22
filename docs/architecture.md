# Architecture

## System Overview

```
Browser (React PWA)
    │  HTTPS / JSON
    ▼
BeeHive.API          ← Controllers, Middleware, JWT auth
    │
BeeHive.Application  ← Services, DTOs, Validators, AutoMapper
    │
BeeHive.Domain       ← Entities, Enums, Interfaces
    │
BeeHive.Infrastructure ← EF Core DbContext, Repositories, UoW
    │
SQL Server (local)
```

## Dependency Rules

```
API → Application → Domain        (allowed)
Infrastructure → Domain           (allowed)
API → Infrastructure              (via DI only, never direct)
Domain → anything else            (FORBIDDEN)
Application → Infrastructure      (FORBIDDEN — use interfaces)
```

## Backend Layers

### BeeHive.Domain
- `BaseEntity`: `Id (Guid)`, `CreatedAt`, `UpdatedAt`
- All entities inherit `BaseEntity`
- Contains enums, no logic beyond simple property rules
- **No dependencies on other layers**

### BeeHive.Infrastructure
- `BeeHiveDbContext` — EF Core, auto-stamps `UpdatedAt` on `SaveChangesAsync`
- `Repository<T>` — generic base with CRUD + predicate search
- Concrete repos extend the generic with domain-specific queries
- `UnitOfWork` — single `SaveChangesAsync`, lazy-initialized repos
- Database auto-migrates and seeds on startup

### BeeHive.Application
- One folder per domain feature: `Apiaries/`, `Beehives/`, `Inspections/`, `Diets/`, `Todos/`, `Auth/`, `Admin/`, `Weather/`
- Each folder contains: `IXxxService`, `XxxService`, `XxxDto`, `XxxValidator`
- `Common/`: `MappingProfile`, `ApplicationExceptions`, `QrCodeService`
- Services receive `IUnitOfWork` via constructor injection

### BeeHive.API
- Controllers: thin — validate input → call service → return HTTP result
- `GlobalExceptionMiddleware` — maps exceptions to Problem Details (RFC 7807)
- JWT Bearer auth — all endpoints require `[Authorize]` except `POST /api/auth/login`
- CORS: `localhost:5173` (Vite dev) and `localhost:4200` (Angular legacy)

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
| DB connection | `appsettings.json` | SQL Server, trusted auth |
| JWT expiry | `appsettings.json` | 480 min (8h) |
| JWT claims | `AuthService` | userId, email, role, organizationId |
| Dev API proxy | `vite.config.ts` | `/api` → `https://localhost:62647` |
| PWA theme | `vite.config.ts` | Amber `#d97706` |
