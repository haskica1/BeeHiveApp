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
