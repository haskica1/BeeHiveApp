# Architecture Decision Records

> This file is append-only. Never remove or edit past decisions.
> Format: Decision → Why → Alternatives considered.

---

## ADR-001: Clean Architecture for Backend

**Decision:** Four-layer Clean Architecture (API → Application → Domain → Infrastructure).

**Why:** Enforces strict separation of concerns. Business logic is isolated from EF Core and HTTP. Each layer is independently testable. Scales well as new features are added.

**Alternatives considered:**
- MVC monolith — simpler but mixes concerns and becomes unmaintainable quickly.
- CQRS / MediatR — considered but added unnecessary ceremony for this project scale.

---

## ADR-002: Repository + Unit of Work Pattern

**Decision:** Generic `Repository<T>` base with domain-specific extensions, coordinated by `IUnitOfWork`.

**Why:** Decouples services from EF Core. Simplifies testing by allowing mock repos. `UnitOfWork` ensures a single `SaveChangesAsync()` call per operation, preventing partial saves.

**Alternatives considered:**
- Direct DbContext in services — faster to write but leaks EF Core into Application layer.
- Dapper — considered for read performance, but EF Core is sufficient at current scale.

---

## ADR-003: JWT Authentication (Stateless)

**Decision:** HS256 JWT tokens, 8-hour expiry, stored in localStorage on the client.

**Why:** Stateless auth fits a REST API — no session store needed. Simple to implement and scale.

**Alternatives considered:**
- Cookie-based sessions — requires server-side session store, adds complexity.
- Refresh token rotation — future consideration if security requirements tighten.

---

## ADR-004: FluentValidation for Input Validation

**Decision:** All DTO validation via `AbstractValidator<T>` classes, registered with `AddFluentValidationAutoValidation()`.

**Why:** Keeps validation logic out of controllers. Declarative rules are easy to read and test. Auto-validation integrates with ModelState, so no manual validator calls needed.

**Alternatives considered:**
- DataAnnotations — less expressive, harder to unit test, limited for complex rules.

---

## ADR-005: AutoMapper for Entity ↔ DTO Conversion

**Decision:** All mapping defined in `MappingProfile.cs` using AutoMapper 13.

**Why:** Eliminates repetitive manual mapping code. Centralized in one place. Works naturally with the service layer pattern.

**Alternatives considered:**
- Manual mapping methods — more explicit but verbose; chosen as fallback only for complex projections.
- Mapster — similar capability, AutoMapper already established in project.

---

## ADR-006: React Query (TanStack Query) for Server State

**Decision:** All data fetching and caching via TanStack Query v5. No Redux or Zustand.

**Why:** Server state (API data) is fundamentally different from UI state. React Query handles caching, stale-time, invalidation, and loading/error states out of the box. Eliminates boilerplate `useEffect` + `useState` patterns.

**Alternatives considered:**
- Redux Toolkit Query — overkill for this app's complexity.
- SWR — similar to React Query but less feature-rich.
- Manual fetch + useState — rejected; error-prone and verbose.

---

## ADR-007: React Hook Form for Forms

**Decision:** All forms use React Hook Form. No controlled `useState` per field.

**Why:** Uncontrolled inputs with ref-based state improve performance. Built-in validation integration. Less re-rendering than fully controlled forms.

**Alternatives considered:**
- Formik — similar but heavier bundle, more verbose.
- Plain controlled components — fine for tiny forms, doesn't scale.

---

## ADR-008: Feature-Based Folder Structure (Frontend)

**Decision:** `features/<domain>/` for pages, `core/services/` for API logic, `shared/components/` for reusable UI.

**Why:** Co-locates everything related to a feature. Easy to find, add, or delete a feature without touching unrelated files.

**Alternatives considered:**
- Type-based structure (`pages/`, `components/`, `hooks/`) — becomes hard to navigate as features grow.

---

## ADR-009: Open-Meteo for Weather (No API Key)

**Decision:** Weather forecasts fetched from `api.open-meteo.com` using apiary coordinates.

**Why:** Free, no API key required, 7-day forecast at sufficient precision for beekeeping decisions.

**Alternatives considered:**
- OpenWeatherMap — requires API key, costs money at scale.
- WeatherAPI — similar; adds key management overhead.

---

## ADR-010: PWA with Vite PWA Plugin

**Decision:** App is a Progressive Web App using `vite-plugin-pwa` with Workbox NetworkFirst caching.

**Why:** Beekeepers work in fields with unreliable connectivity. PWA enables offline-capable UI and installability on mobile devices without an app store.

**Alternatives considered:**
- Native mobile apps — too costly to build and maintain.
- Simple SPA without PWA — no offline support.

---

## ADR-011: GlobalExceptionMiddleware + Problem Details

**Decision:** Single middleware catches all exceptions and maps to RFC 7807 Problem Details JSON.

**Why:** Consistent error format across all endpoints. Controllers stay clean — they never need try/catch. Frontend can rely on a predictable error shape.

**Alternatives considered:**
- Per-controller try/catch — repetitive, easy to forget in new controllers.
- Custom error response format — Problem Details is an HTTP standard, better for interoperability.

---

## ADR-012: SystemAdmin Role for Platform Management

**Decision:** Two roles: `Admin` (org-level) and `SystemAdmin` (platform-level). SystemAdmin manages organizations and users via `/api/admin`.

**Why:** Multi-tenant design requires a super-admin to provision organizations. Separates operational concerns from business logic.

**Alternatives considered:**
- Single role — no separation of platform vs. org management.
- Fine-grained permissions — over-engineered for current user base.

---

## ADR-013: Dedicated `BeeHive.Entity` Persistence Project

**Decision:** Move all data access (DbContext, entity configurations, repositories, UnitOfWork, migrations) into a dedicated `BeeHive.Entity` project. `BeeHive.Infrastructure` is slimmed to external services (email).

**Why:** Makes the persistence boundary explicit and single-purpose. Migrations and EF Core concerns live in one clearly-named project. Infrastructure no longer mixes data access with outbound integrations.

**Notes:** Migration IDs were preserved during the move, so `__EFMigrationsHistory` still matches — no database change. EF CLI now targets `--project BeeHive.Entity --startup-project BeeHive.API`.

**Alternatives considered:**
- Keep everything in `Infrastructure` — the clean-architecture default, but conflates persistence with other infrastructure.

---

## ADR-014: Centralized Authorization (`ICurrentUser` + `IAccessGuard`)

**Decision:** All tenant/resource ownership checks live in the service layer via a single `IAccessGuard`, fed by an `ICurrentUser` abstraction over JWT claims. Controllers keep only coarse `[Authorize(Roles = …)]` gating. `ForbiddenAccessException` maps to 403.

**Why:** The previous design scattered authorization across controllers with duplicated, inconsistent checks — which had allowed cross-tenant access (an OrgAdmin could read/modify another organization's data) and IDOR on by-id reads. A single source of truth fixes the holes and prevents regressions.

**Alternatives considered:**
- Per-controller checks — the original approach; error-prone and duplicated.
- ASP.NET resource-based authorization handlers — heavier; the service-layer guard is simpler and reuses loaded entities.

---

## ADR-015: Role Rename for Clarity

**Decision:** Rename roles `Admin → ApiaryAdmin`, `OrgAdmin → OrganizationAdmin`, `User → Beekeeper` (`SystemAdmin` unchanged). Numeric enum values are preserved (1–4).

**Why:** The old names were misleading — "Admin" was actually the *narrowest* (apiary-level) role. The new names state the scope plainly.

**Notes:** Breaking change to the JWT role claim string (users must re-log in); no database migration needed since persisted ints are unchanged. Frontend role checks updated in lockstep.

---

## ADR-016: One Type Per File

**Decision:** Every public type (enum, interface, exception, service, DTO, validator, EF configuration, repository) lives in its own file. Validators are co-located per feature under `Features/<F>/Validators/`.

**Why:** Faster navigation and clearer single-responsibility. Supersedes the earlier convention of grouping a feature's DTOs/validators into one file.

**Alternatives considered:**
- Grouped-per-feature files — fewer files, but harder to locate a specific type as features grow.

## ADR-017: Refresh-Token Rotation (supersedes the auth part of ADR-003)

**Decision:** Access token shortened to 30 minutes; a rotating refresh token (14 days) is issued alongside it. Refresh tokens are stored **hashed** (SHA-256); every refresh revokes the presented token and links its replacement; presenting an already-rotated token revokes the user's entire active set (theft detection). Client keeps tokens in localStorage and refreshes via a single-flight 401 interceptor.

**Why:** ADR-003's 8-hour token was a large theft window with no revocation story. Rotation bounds the damage of a leaked access token to 30 minutes and makes refresh-token theft self-defeating.

**Notes:** localStorage (vs. httpOnly cookie) is a known XSS trade-off, accepted for now because rotation + reuse detection bound the damage; revisit if requirements tighten. Contract locked by `AuthServiceTests`.

---

## ADR-018: Explicit Validation Calls in Controllers (supersedes ADR-004's auto-validation)

**Decision:** Controllers call `await _validator.ValidateAsync(dto)` explicitly and return `BadRequest(validation.ToDictionary())`. FluentValidation's ASP.NET auto-validation is deliberately **not** enabled.

**Why:** Preserves the exact `errors`-dictionary response shape the frontend forms rely on, and follows FluentValidation's own guidance (auto-validation is deprecated by its authors). The per-action boilerplate (~6 lines) is the accepted cost; a shared action filter is a possible future refinement, not a requirement.

---

## ADR-019: Per-Feature Mapping Profiles + Manual Mapping for Computed DTOs (supersedes ADR-005's single MappingProfile)

**Decision:** Each feature owns a `<Feature>MappingProfile`. DTOs whose fields are computed (Diets progress counts, Admin aggregates) are mapped **manually** in the service instead of forcing AutoMapper.

**Why:** One shared `MappingProfile.cs` had become a merge hotspot and hid feature coupling. Manual mapping where projections are computed keeps the intent visible; AutoMapper remains the default for plain property copies.

---

## ADR-020: No Secrets in the Repository; Dev-Only Demo Seed + Production Bootstrap Admin

**Decision:** `appsettings.json` carries empty placeholders only. Real values come from environment variables in production (`Jwt__Secret`, `Smtp__Password`, `Groq__ApiKey`, `ConnectionStrings__DefaultConnection`, `Bootstrap__SysAdminEmail`, `Bootstrap__SysAdminPassword`) and from `appsettings.Development.json`/user-secrets locally. `Program.cs` fails fast when required values are missing. Demo users (public passwords) are seeded **only in Development**; every production startup locks the demo accounts (random password hash + refresh-token revocation) and provisions the real SystemAdmin from the `Bootstrap:*` values.

**Why:** The repository is public. A committed JWT secret means anyone can forge tokens; committed demo SystemAdmin credentials meant anyone could log into production. Both actually happened and were rotated on 2026-07-02.

---

## ADR-021: Notification Email via In-Memory Queue + Background Worker

**Decision:** `NotificationService` persists the in-app notification, then enqueues `(userId, title, message)` onto an unbounded `System.Threading.Channels` queue. `EmailNotificationWorker` (a `BackgroundService` in Infrastructure) dequeues, resolves the recipient in its own DI scope, and sends via MailKit. Email is skipped unless `Smtp:Host` and `Smtp:Password` are configured.

**Why:** Synchronous SMTP inside the request pipeline caused request timeouts, so delivery had been disabled entirely. Queue + worker restores email without adding a message broker; losing queued mail on process shutdown is acceptable because the in-app notification is already persisted (email is best-effort).

**Alternatives considered:**
- Hangfire / Quartz — persistent retries, but a heavy dependency for best-effort mail.
- `Task.Run` fire-and-forget — no backpressure, swallows failures, unscoped DbContext hazards.

---

## ADR-022: Lean List Payloads — Counts in SQL, QR Codes On Demand

**Decision:** List endpoints never carry derived heavy data: inspection/beehive counts are computed in the database (`GROUP BY` / `Select(x => x.Collection.Count)`) instead of `Include`-ing full child rows, and the Base64 QR PNG lives only on the beehive **detail** DTO plus a dedicated `GET /api/beehives/by-apiary/{id}/qr-codes` endpoint used by label export.

**Why:** Apiary/beehive lists previously loaded every inspection row of the organization and a ~KB QR blob per hive on every request — the heaviest queries in the app, on the most-visited pages, only to display counts.

---

## ADR-023: Bosnian as the Single UI Language (`BsLabels`)

**Decision:** All user-facing strings the API produces — `*Name` enum label fields, calendar labels, and notification titles/messages — are Bosnian, sourced from `Common/Localization/BsLabels` on the backend and the matching label maps in `core/models/index.ts` on the frontend. Logs, code, docs, and Swagger stay English.

**Why:** The UI is Bosnian; mixed English fragments (enum `.ToString()`, English notifications) looked broken. `BsLabels` already existed for Stats — it is now the single source instead of per-service formatting.

**Alternatives considered:**
- Full i18n framework — over-engineered while the product is single-language; revisit if a second language is needed.

---

## ADR-024: Extracted AI Client Seam for the Advisor (SPEC-01)

**Decision:** Transcription is extracted from `VoiceParsingService` into a shared
`ITranscriptionService` / `GroqTranscriptionService` (Whisper large-v3), and advisor chat goes through a
thin `IAdvisorAiClient` / `GroqAdvisorAiClient` wrapper over Groq chat completions. `VoiceParsingService`
now consumes `ITranscriptionService` (no behavior change). On the frontend, `useVoiceInput` moved from
`features/inspections/` to `core/hooks/` since it is now shared; the upload endpoint stays in each
caller's service (inspections → `parse-voice`, advisor → `/advisor/transcribe`).

**Why:** The advisor reuses the exact Groq transcription the inspection flow already had, and hiding the
chat call behind an interface makes `AdvisorService` unit-testable (Groq mocked) — the previous structure
had transcription and the model call welded inside `VoiceParsingService`. No new AI provider or secret;
reuses `Groq:ApiKey`.

**Alternatives considered:**
- Duplicate the transcription code in the advisor — divergent prompts/behavior over time.
- Call Groq directly from `AdvisorService` — untestable without hitting the network.

---

## ADR-025: `react-markdown` for Learning Article Rendering (SPEC-06)

**Decision:** Learning-topic bodies are authored as markdown and rendered with **`react-markdown`**
(the spec-flagged new dependency, approved with SPEC-06 implementation). No raw-HTML plugins
(`rehype-raw` etc.) are added — react-markdown's default escaping renders `<script>` and any embedded
HTML as inert text, which is the XSS guard for admin-authored content. Styling goes through a shared
`MarkdownArticle` component (`features/learning/`) with Tailwind-styled element mappings, reused by the
reader page and the admin preview. To keep the PWA precache working (workbox 2 MiB per-file limit),
`react-markdown` and `recharts` are split into their own vendor chunks via `manualChunks` in
`vite.config.ts`.

**Why:** Markdown is the right authoring format for admin-written articles (headings, lists, tables),
and hand-rolling a renderer is exactly the kind of parsing/XSS surface a maintained library eliminates.

**Alternatives considered:**
- `marked`/`markdown-it` + `dangerouslySetInnerHTML` — requires a separate sanitizer (DOMPurify) and
  careless use is an XSS foot-gun; react-markdown renders to React elements, never raw HTML.
- Tailwind `@tailwindcss/typography` plugin for styling — another dependency for what 15 element
  mappings in one component already do.
