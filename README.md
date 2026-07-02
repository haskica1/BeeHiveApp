# BeeHive App 🐝

Multi-tenant beekeeping management SaaS: organizations manage apiaries → beehives →
inspections, feeding programs (diets), todos, and expenses. Includes QR-code hive scanning,
voice-note inspections (Whisper + Llama via Groq), 7-day weather per apiary (Open-Meteo),
statistics, calendar, and in-app + email notifications. UI language: Bosnian.

**Stack:** .NET 10 (Clean Architecture, EF Core + PostgreSQL) · React 18 + TypeScript + Vite +
TanStack Query v5 + Tailwind (PWA)

---

## Repository Layout

```
backend/
  BeeHive.API/              ← controllers, middleware, JWT auth, Program.cs
  BeeHive.Application/      ← services, DTOs, validators, IAccessGuard (feature slices)
  BeeHive.Domain/           ← entities + enums (no dependencies)
  BeeHive.Entity/           ← EF Core DbContext, repositories, UnitOfWork, migrations
  BeeHive.Infrastructure/   ← email (MailKit) + background email worker
  BeeHive.Application.Tests/← xunit unit tests (authorization, diets, auth)
frontend/                   ← React SPA/PWA
docs/                       ← architecture, decisions (ADRs), API contracts, guidelines
```

## Prerequisites

- .NET 10 SDK
- Node.js 20+
- PostgreSQL 15+ running locally (default connection expects `localhost:5432`,
  database `BeeAppDB`, user `postgres` — see `backend/BeeHive.API/appsettings.Development.json`)

## Running Locally

**Backend** (http://localhost:62648, Swagger at `/swagger`):

```bash
cd backend/BeeHive.API
dotnet run
```

The database is migrated and demo users are seeded automatically on startup (Development only).
Demo logins: `sysadmin@beehive.com / SysAdmin123!`, `orgadmin@goldenhive.com / OrgAdmin123!`,
`admin@goldenhive.com / Admin123!`, `user1@goldenhive.com / User123!`.

**Frontend** (http://localhost:5173, proxies `/api` to the local backend):

```bash
cd frontend
npm install
npm run dev
```

**Tests:**

```bash
cd backend
dotnet test BeeHive.Application.Tests/BeeHive.Application.Tests.csproj
```

## Configuration & Secrets

`appsettings.json` is committed **without secrets** (empty placeholders). The app fails fast at
startup if a required value is missing.

| Key | Purpose | Local dev | Production (env var) |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL | `appsettings.Development.json` | `ConnectionStrings__DefaultConnection` |
| `Jwt:Secret` | HS256 signing key (≥ 32 chars) | dev-only value in `appsettings.Development.json` | `Jwt__Secret` |
| `Smtp:Password` | Gmail app password for notification email | user-secrets (optional — email is skipped if unset) | `Smtp__Password` |
| `Groq:ApiKey` | Voice-note transcription/parsing | user-secrets (optional) | `Groq__ApiKey` |
| `Bootstrap:SysAdminEmail` / `Bootstrap:SysAdminPassword` | Provisions the production SystemAdmin | not needed | `Bootstrap__SysAdminEmail`, `Bootstrap__SysAdminPassword` |
| `AllowedOrigins` | CORS (comma-separated) | defaults include `localhost:5173` | `AllowedOrigins` |
| `FrontendUrl` | Base URL embedded in hive QR codes | default | `FrontendUrl` |

Local secrets via user-secrets:

```bash
cd backend/BeeHive.API
dotnet user-secrets set "Groq:ApiKey" "<key>"
dotnet user-secrets set "Smtp:Password" "<gmail-app-password>"
```

**Production note:** demo accounts are locked (random password + revoked refresh tokens) on every
production startup; the real SystemAdmin comes exclusively from the `Bootstrap__*` env vars.

## Deployment

- **Backend:** Render — set the env vars above; TLS terminates at Render's proxy; liveness probe at `/health`.
- **Frontend:** Vercel — set `VITE_API_URL` to the deployed API base URL (e.g. `https://<app>.onrender.com/api`).

## Documentation

Start with [docs/claude.md](docs/claude.md) (project rules), then
[docs/architecture.md](docs/architecture.md), [docs/decisions.md](docs/decisions.md) (ADRs),
[docs/context.md](docs/context.md) (implemented-feature inventory), and
[docs/api-contracts.md](docs/api-contracts.md).
