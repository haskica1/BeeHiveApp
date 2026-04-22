# API Contracts

## Base URL

- Dev: `https://localhost:62647/api`
- Frontend proxy: `/api` → backend (configured in `vite.config.ts`)

## Authentication

All endpoints require `Authorization: Bearer <jwt>` except `POST /api/auth/login`.

**JWT Claims:**

| Claim | Key | Description |
|---|---|---|
| User ID | `userId` | Guid |
| Email | `email` | string |
| Role | `role` | `Admin` or `SystemAdmin` |
| Organization | `organizationId` | Guid |

**Token lifetime:** 480 minutes (8 hours)

---

## Standard Response Format

### Success
HTTP `200 OK`, `201 Created`, `204 No Content` — response body is the DTO directly (not wrapped).

### Error — Problem Details (RFC 7807)

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Not Found",
  "status": 404,
  "detail": "Apiary with id '...' was not found."
}
```

| Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 |
| `ValidationException` | 400 (includes `errors` map) |
| `BusinessRuleException` | 422 |
| Unhandled | 500 |

---

## Endpoints

### Auth

| Method | Path | Auth | Description |
|---|---|---|---|
| POST | `/auth/login` | Public | Returns token + user |

**Login request:** `{ email, password }`
**Login response:** `{ token, userId, email, role, organizationId }`

---

### Apiaries

| Method | Path | Returns |
|---|---|---|
| GET | `/apiaries` | `ApiaryDto[]` |
| GET | `/apiaries/{id}` | `ApiaryDetailDto` (includes beehives) |
| POST | `/apiaries` | `201 + ApiaryDto` |
| PUT | `/apiaries/{id}` | `200 + ApiaryDto` |
| DELETE | `/apiaries/{id}` | `204` |
| GET | `/apiaries/{id}/weather` | `WeatherForecastDto` |

**ApiaryDto:** `{ id, name, location, latitude, longitude, organizationId, createdAt }`
**ApiaryDetailDto:** extends ApiaryDto + `beehives: BeehiveDto[]`

---

### Beehives

| Method | Path | Returns |
|---|---|---|
| GET | `/beehives/by-apiary/{apiaryId}` | `BeehiveDto[]` |
| GET | `/beehives/{id}` | `BeehiveDetailDto` (includes inspections) |
| POST | `/beehives` | `201 + BeehiveDto` |
| PUT | `/beehives/{id}` | `200 + BeehiveDto` |
| DELETE | `/beehives/{id}` | `204` |

**BeehiveDto:** `{ id, name, type, material, apiaryId, qrCode (Base64 PNG), uniqueId (Guid), createdAt }`
**BeehiveDetailDto:** extends BeehiveDto + `inspections: InspectionDto[]`

---

### Inspections

| Method | Path | Returns |
|---|---|---|
| GET | `/inspections/by-beehive/{beehiveId}` | `InspectionDto[]` (newest first) |
| GET | `/inspections/{id}` | `InspectionDto` |
| POST | `/inspections` | `201 + InspectionDto` |
| PUT | `/inspections/{id}` | `200 + InspectionDto` |
| DELETE | `/inspections/{id}` | `204` |

**InspectionDto:** `{ id, beehiveId, date, temperature, honeyLevel, broodStatus, notes, createdAt }`

---

### Todos

| Method | Path | Returns |
|---|---|---|
| GET | `/todos/by-apiary/{apiaryId}` | `TodoDto[]` |
| GET | `/todos/by-beehive/{beehiveId}` | `TodoDto[]` |
| GET | `/todos/{id}` | `TodoDto` |
| POST | `/todos` | `201 + TodoDto` |
| PUT | `/todos/{id}` | `200 + TodoDto` |
| DELETE | `/todos/{id}` | `204` |

**TodoDto:** `{ id, title, description, priority, dueDate, isCompleted, apiaryId?, beehiveId? }`

---

### Diets

| Method | Path | Returns |
|---|---|---|
| GET | `/diets/by-beehive/{beehiveId}` | `DietDto[]` |
| GET | `/diets/{id}` | `DietDetailDto` (includes feeding entries) |
| POST | `/diets` | `201 + DietDto` |
| PUT | `/diets/{id}` | `200 + DietDto` |
| DELETE | `/diets/{id}` | `204` |
| POST | `/diets/{id}/complete-early` | `200` — requires `{ comment }` |
| POST | `/diets/{id}/entries/{entryId}/complete` | `200` |

**DietDto:** `{ id, beehiveId, startDate, reason, duration, frequency, foodType, status, createdAt }`
**DietDetailDto:** extends DietDto + `feedingEntries: FeedingEntryDto[]`

---

### Admin (SystemAdmin only)

| Method | Path | Description |
|---|---|---|
| GET | `/admin/organizations` | List all orgs with details |
| GET | `/admin/organizations/{id}` | Org detail |
| POST | `/admin/organizations` | Create org |
| PUT | `/admin/organizations/{id}` | Update org |
| DELETE | `/admin/organizations/{id}` | Delete org |
| GET | `/admin/users` | List all users with org |
| POST | `/admin/users` | Create user |
| PUT | `/admin/users/{id}` | Update user |
| DELETE | `/admin/users/{id}` | Delete user |

---

## Enum Reference

```
BeehiveType:     Langstroth | DadantBlatt | Warré | TopBar | Other
BeehiveMaterial: Wood | Plastic | Polystyrene
HoneyLevel:      Low | Medium | High
TodoPriority:    Low | Medium | High
DietStatus:      NotStarted | InProgress | Completed | StoppedEarly
DietReason:      LackOfFood | WinterFeeding | SpringStimulation | (+ 6 more)
FoodType:        SugarSyrup | Fondant | Pollen | ProteinPatties | Custom
UserRole:        Admin | SystemAdmin
```
