# Ignore — Do Not Touch

> These files and areas are stable and must not be modified unless explicitly directed.
> Accidental changes here can break migrations, auth, or data integrity.

---

## Database Migrations

**Path:** `backend/BeeHive.Infrastructure/Migrations/`

**Rule:** Never edit, rename, or delete existing migration files.

**Why:** EF Core migration history is sequential. Editing past migrations corrupts the schema version chain and breaks `dotnet ef database update` for all environments.

**Allowed:** Adding new migration files only (`dotnet ef migrations add <Name>`).

---

## BeeHiveDbContext — Existing Entity Configurations

**Path:** `backend/BeeHive.Infrastructure/BeeHiveDbContext.cs`

**Rule:** Do not change existing `OnModelCreating` configurations, column types, or table names.

**Why:** Any change requires a migration. Accidental renames cause data loss on `UPDATE`.

**Allowed:** Adding new `DbSet<T>` properties and new entity configurations for new tables.

---

## GlobalExceptionMiddleware

**Path:** `backend/BeeHive.API/Middleware/GlobalExceptionMiddleware.cs`

**Rule:** Do not add new exception types or change existing HTTP status mappings without a deliberate decision.

**Why:** The exception-to-status mapping is the contract the frontend relies on. Changing it silently breaks error handling across all features.

**Allowed:** Adding a mapping for a genuinely new exception type; update `decisions.md` when doing so.

---

## MappingProfile — Existing Maps

**Path:** `backend/BeeHive.Application/Common/MappingProfile.cs`

**Rule:** Do not modify existing `CreateMap` entries. Only append new ones.

**Why:** Changing an existing map can silently break all endpoints that use it, with no compile-time error.

**Allowed:** Adding new `CreateMap` lines for new entities or DTOs.

---

## AuthService — JWT Claim Structure

**Path:** `backend/BeeHive.Application/Auth/AuthService.cs`

**Rule:** Do not rename or remove existing JWT claims (`userId`, `email`, `role`, `organizationId`).

**Why:** The frontend reads these claims from localStorage. The backend reads them in every controller via `User.FindFirst(...)`. Renaming one breaks both without a compile error.

**Allowed:** Adding new claims if a feature needs them; document in `api-contracts.md`.

---

## apiClient.ts — Interceptor Logic

**Path:** `frontend/src/core/services/apiClient.ts`

**Rule:** Do not change the 401 handling logic (auto-logout + redirect) or the token attachment pattern.

**Why:** This is the auth backbone of the frontend. Changing it breaks session management for every request in the app.

**Allowed:** Adding new interceptors for new cross-cutting concerns (e.g., request ID header). Do not remove existing ones.

---

## core/models/index.ts — Existing Interfaces

**Path:** `frontend/src/core/models/index.ts`

**Rule:** Do not rename or remove existing interface properties. TypeScript may not catch all usages.

**Why:** Interface properties are used across many components and services. Renaming silently breaks string-keyed accesses and form field bindings.

**Allowed:** Adding new properties to existing interfaces, adding new interfaces.

---

## DatabaseInitializer (Seed Data)

**Path:** `backend/BeeHive.Infrastructure/DatabaseInitializer.cs` (or similar)

**Rule:** Do not change the default SystemAdmin credentials or the seed organization structure.

**Why:** Dev/test environments depend on known seed data. Changing it breaks existing local setups.

**Allowed:** Adding new seed data for new features; document in `context.md`.

---

## Summary Table

| Area | Path | Rule |
|---|---|---|
| Migrations | `Infrastructure/Migrations/` | Never edit existing files |
| DbContext configurations | `BeeHiveDbContext.cs` | Never change existing entity configs |
| Exception middleware | `Middleware/GlobalExceptionMiddleware.cs` | Never change existing status mappings |
| AutoMapper | `MappingProfile.cs` | Never modify existing maps |
| JWT claims | `Auth/AuthService.cs` | Never rename or remove claims |
| Axios interceptors | `apiClient.ts` | Never change auth/401 logic |
| Frontend models | `core/models/index.ts` | Never rename/remove interface properties |
| Seed data | `DatabaseInitializer.cs` | Never change default credentials |
