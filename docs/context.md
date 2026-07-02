# Context — Current System State

> This file reflects what is **actually implemented** as of 2026-07-02.
> Update this file whenever a feature is completed or removed.
> Use this to avoid re-implementing existing functionality.

---

## Implemented Features

### Authentication & Accounts
- `POST /api/auth/login` — email + password, returns access token (**30 min**) + rotating refresh token (**14 days**)
- `POST /api/auth/register` — **self-service sign-up**: creates a new Organization and its OrganizationAdmin, auto-login
- `POST /api/auth/refresh` — rotates the refresh token; **reuse of a rotated token revokes the user's whole active set**
- `POST /api/auth/logout` — revokes the presented refresh token (idempotent)
- Rate limiting per client IP: login 5/min, register 5/min, refresh 20/min → `429`
- Passwords hashed with BCrypt; refresh tokens stored **hashed** (SHA-256)
- JWT claims: `sub` (int user id), `email`, `role`, `jti`, `organizationId?`, `apiaryId?`
- **Four roles** (`UserRole`): `SystemAdmin` (platform), `OrganizationAdmin` (whole org),
  `ApiaryAdmin` (one assigned apiary), `Beekeeper` (only explicitly assigned beehives)
- All resource authorization is centralized in `IAccessGuard` (Application/Common/Security)
- Frontend: `AuthContext`, `ProtectedRoute`, `AdminRoute`, `RoleRoute`, `SmartRedirect`;
  `apiClient` does single-flight refresh-on-401 with request replay
- Production bootstrap: SystemAdmin provisioned from `Bootstrap:SysAdminEmail/Password` env vars;
  demo accounts are seeded **only in Development** and locked on every production startup

### Organizations & Users (SystemAdmin)
- Full CRUD via `/api/admin/organizations` and `/api/admin/users`
  (`OrganizationsAdminController` + `UsersAdminController`)
- Role/org/apiary consistency rules enforced in `AdminService`
- Demo data (2 orgs, apiaries, hives) seeded via EF `HasData` migrations

### Organization Members (`/api/org/*`)
- OrganizationAdmin/ApiaryAdmin manage members: create ApiaryAdmin/Beekeeper accounts,
  assign apiaries to admins, assign beehives to beekeepers (`OrgManagementService`)

### Apiaries
- Full CRUD via `/api/apiaries`, org-scoped; ApiaryAdmin sees only their apiary,
  Beekeeper only apiaries containing their assigned hives
- `latitude`/`longitude` + map picker (react-leaflet)
- Weather: `GET /api/apiaries/{id}/weather` — 7-day forecast + current conditions from Open-Meteo (no API key)

### Beehives
- Full CRUD via `/api/beehives`; hive lists return inspection **counts** (grouped query, no inspection rows)
- Types/materials use Bosnian display labels (`BsLabels`)
- Auto-generated `uniqueId` (Guid) + QR code (Base64 PNG, QRCoder) on creation
- QR codes: only in the **detail** DTO; bulk label export via `GET /api/beehives/by-apiary/{id}/qr-codes`
- Public scan flow: `GET /api/beehives/scan/{uniqueId}` (anonymous) + `/scan/:uniqueId` route + in-app QR scanner (`@zxing`)
- `POST /api/beehives/regenerate-qr-codes` (SystemAdmin) regenerates all QR codes after a frontend URL change

### Inspections
- Full CRUD via `/api/inspections`; temperature auto-filled from the apiary's current weather (best-effort)
- **Voice input**: `POST /api/inspections/parse-voice` — audio → Groq Whisper large-v3 transcription (BCS)
  → Llama 3.3 70B field extraction → `{date, honeyLevel, broodStatus, notes}` + transcript.
  15 MB size limit + 10/min rate limit. Frontend records via `useVoiceInput`.

### Diets (Feeding Programs)
- Full CRUD via `/api/diets`; entries auto-generated from duration + frequency
- State machine: `NotStarted → InProgress → Completed | StoppedEarly`
- `POST /api/diets/{id}/complete-early` (requires comment); per-entry completion endpoint
- Delete allowed only before start with no completed entries
- Covered by unit tests (`BeeHive.Application.Tests`)

### Todos
- Full CRUD via `/api/todos`; scoped to either an Apiary or a Beehive
- Priority (Low/Medium/High), optional due date, optional assignee
- `GET /api/todos/open` — role-scoped open todos; assignable-users endpoint per beehive

### Expenses
- Full CRUD via `/api/expenses` with line items (`ExpenseItem`)
- Client-side receipt scanning (`ReceiptScanPage`): tesseract.js OCR (`hrv` model) + heuristic line parser

### Calendar & Stats
- `GET /api/calendar` — role-scoped todos + feeding entries (Bosnian labels)
- `GET /api/stats` — org-scoped (platform-wide for SystemAdmin): totals, distributions,
  12-month inspection/temperature series, top hives (Recharts on the frontend)

### Notifications
- In-app bell (30 s polling) + email via background queue
- 9 `NotificationType`s fired on account/org/apiary/beehive assignment changes, hive creation, todo creation
- Email: `NotificationService` enqueues → `EmailNotificationWorker` (BackgroundService, Channel)
  resolves the recipient and sends via MailKit — **SMTP never blocks a request**
- Email silently skipped unless `Smtp:Host` + `Smtp:Password` are configured
- `POST /api/notifications/test-email` (SystemAdmin) — direct SMTP test
- All notification texts are in Bosnian

### Profile
- `GET/PUT /api/profile` — name/email + password change

---

## Infrastructure / Cross-Cutting

| Concern | Implementation |
|---|---|
| Database | **PostgreSQL** (Npgsql), EF Core 10, auto-migrate on startup |
| Projects | API → Application → Domain; **Entity** (persistence) + Infrastructure (email) implement Application interfaces |
| Validation | FluentValidation, **explicit `ValidateAsync` in controllers** (no auto-validation — see ADR-010) |
| Mapping | AutoMapper per-feature profiles; manual mapping where DTOs have computed fields (Diets, Admin) |
| Error handling | `GlobalExceptionMiddleware` → Problem-Details-style JSON; **exception details only in Development** |
| Auth | JWT Bearer HS256, 30 min access + 14 d refresh rotation |
| Secrets | **Not in the repo.** Env vars in production (`Jwt__Secret`, `Smtp__Password`, `Groq__ApiKey`, `ConnectionStrings__DefaultConnection`, `Bootstrap__*`); `appsettings.Development.json` / user-secrets locally |
| Rate limiting | Fixed-window per IP: login/register 5/min, refresh 20/min, parse-voice 10/min |
| Health check | `GET /health` (liveness, used by Render) |
| CORS | `AllowedOrigins` config (comma-separated), overridable via env var |
| API docs | Swagger UI at `/swagger` (all environments) |
| Frontend caching | TanStack Query v5; 30 s notification polling; PWA (Workbox NetworkFirst, 24 h) |
| Localization | UI + API `*Name` fields + notifications in Bosnian (`BsLabels` backend, label maps frontend) |
| Tests | `BeeHive.Application.Tests` (xunit + NSubstitute): AccessGuard authorization matrix, Diet state machine, refresh-token rotation |
| Deployment | Backend: Render (Docker, TLS at proxy). Frontend: Vercel (`VITE_API_URL`). Dev proxy → `http://localhost:62648` |

---

## Pending / Not Yet Implemented

> Add items here when planned but not yet built.

- Multi-language support (UI is Bosnian-only; no i18n framework)
- Reports/analytics export (PDF exists only for QR labels)
- Push notifications (PWA) — currently 30 s polling only
- Integration tests against a real PostgreSQL (unit tests only for now)
- Refresh token in httpOnly cookie (currently localStorage — see ADR-009)
