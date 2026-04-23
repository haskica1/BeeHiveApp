# Context — Current System State

> This file reflects what is **actually implemented** as of 2026-04-23.
> Update this file whenever a feature is completed or removed.
> Use this to avoid re-implementing existing functionality.

---

## Implemented Features

### Authentication
- `POST /api/auth/login` — email + password login, returns JWT
- JWT contains: `userId`, `email`, `role`, `organizationId`
- Token expiry: 8 hours
- Passwords hashed with BCrypt
- Two roles: `Admin`, `SystemAdmin`
- Frontend: `AuthContext`, `ProtectedRoute`, `AdminRoute`, `SmartRedirect`
- No registration flow — users created by SystemAdmin only

### Organizations (SystemAdmin only)
- Full CRUD via `/api/admin/organizations`
- Each organization is an isolated tenant
- Seeded on startup via `DatabaseInitializer`

### Users (SystemAdmin only)
- Full CRUD via `/api/admin/users`
- User belongs to one Organization
- Password set by SystemAdmin on creation

### Apiaries
- Full CRUD via `/api/apiaries`
- Scoped to authenticated user's `organizationId` (extracted from JWT)
- Stores `latitude` + `longitude` for weather
- Weather: `GET /apiaries/{id}/weather` — 7-day forecast from Open-Meteo (no API key)
- Frontend: list page, detail page, create/edit form

### Beehives
- Full CRUD via `/api/beehives`
- Belongs to one Apiary
- Types: Langstroth, DadantBlatt, Warré, TopBar, Other
- Materials: Wood, Plastic, Polystyrene
- Auto-generates `uniqueId` (Guid) and `qrCode` (Base64 PNG) on creation using QRCoder
- Frontend: detail page, create/edit form, QR code display

### Inspections
- Full CRUD via `/api/inspections`
- Belongs to one Beehive
- Records: date, temperature (°C), honey level, brood status, notes
- Returned newest-first
- Frontend: form page (create/edit), embedded list on BeehiveDetailPage

### Diets (Feeding Programs)
- Full CRUD via `/api/diets`
- Belongs to one Beehive
- Auto-generates `FeedingEntries` on creation based on duration + frequency
- Status machine: NotStarted → InProgress → Completed | StoppedEarly
- `POST /diets/{id}/complete-early` — requires comment
- `POST /diets/{id}/entries/{entryId}/complete` — marks one entry done
- Frontend: DietDetailPage, DietFormPage, DietSection (reusable list component)

### Todos
- Full CRUD via `/api/todos`
- Scoped to **either** an Apiary **or** a Beehive (not both, not neither)
- Priority: Low, Medium, High
- Optional due date
- Frontend: TodoSection (reusable component), embedded on ApiaryDetailPage + BeehiveDetailPage

---

## Infrastructure / Cross-Cutting

| Concern | Implementation |
|---|---|
| Database | SQL Server, EF Core 10, auto-migrate on startup |
| Validation | FluentValidation with auto-validation middleware |
| Mapping | AutoMapper 13 — all mappings in `MappingProfile.cs` |
| Error handling | `GlobalExceptionMiddleware` → Problem Details (RFC 7807) |
| Auth | JWT Bearer (HS256), claims-based, 8h expiry |
| CORS | `localhost:5173` (Vite) + `localhost:4200` (legacy) |
| API docs | Swagger UI with JWT button at `/swagger` |
| Frontend caching | React Query, 2min stale time (30min for weather) |
| PWA | Vite PWA Plugin + Workbox NetworkFirst, installable |
| PDF export | jsPDF available (used selectively) |
| Icons | Lucide React |
| Styling | Tailwind CSS 3, honey/amber theme |

---

## Pending / Not Yet Implemented

> Add items here when planned but not yet built.

- Refresh token / silent re-auth
- Notifications / reminders for todo due dates
- QR code scanning to navigate to beehive
- Reports / analytics dashboard
- Multi-language support (Bosnian UI)
- Production deployment configuration
