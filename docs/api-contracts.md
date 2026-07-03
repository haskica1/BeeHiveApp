# API Contracts

## Base URL

- Dev: `https://localhost:62647/api`
- Frontend proxy: `/api` → backend (configured in `vite.config.ts`)

## Authentication

All endpoints require `Authorization: Bearer <jwt>` except
`POST /api/auth/{login, register, refresh, logout}` and `GET /api/beehives/scan/{uniqueId}`.

**Endpoints:**

| Method | Endpoint | Notes |
|---|---|---|
| POST | `/auth/login` | 5/min per IP; returns access + refresh token |
| POST | `/auth/register` | 5/min per IP; creates a new organization + OrganizationAdmin |
| POST | `/auth/refresh` | 20/min per IP; rotates the refresh token (reuse revokes the whole set) |
| POST | `/auth/logout` | revokes the presented refresh token (idempotent) |

**JWT Claims:**

| Claim | Key | Description |
|---|---|---|
| User ID | `sub` | int |
| Email | `email` | string |
| Role | `role` | `SystemAdmin`, `OrganizationAdmin`, `ApiaryAdmin`, or `Beekeeper` |
| Token ID | `jti` | Guid |
| Organization | `organizationId` | int, absent for SystemAdmin |
| Apiary | `apiaryId` | int, only for ApiaryAdmin |

**Token lifetime:** access token 30 minutes; refresh token 14 days (rotating, stored hashed).

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

### Queens

| Method | Path | Returns |
|---|---|---|
| GET | `/beehives/{beehiveId}/queens` | `QueenDto[]` (newest introduction first) |
| POST | `/beehives/{beehiveId}/queens` | `201 + QueenDto` — new queen is Active; existing active queen auto-closed as Replaced (atomic) |
| PUT | `/queens/{id}` | `200 + QueenDto` — `422` when activating while another queen is active |
| DELETE | `/queens/{id}` | `204` |

**QueenDto:** `{ id, beehiveId, year, markColor, markColorName, isMarked, isClipped, origin, originName, status, statusName, introducedDate, endDate?, notes?, createdAt }`

**Create body:** `{ year, markColor?, isMarked, isClipped, origin, introducedDate, notes? }` — `markColor` omitted → derived from `year` (international color code).

---

### Harvests

| Method | Path | Returns |
|---|---|---|
| GET | `/harvests?apiaryId=&year=` | `HarvestDto[]` (role-scoped; incl. `totalKg`, `entryCount`, `apiaryName`, `estimatedRevenue`) |
| GET | `/harvests/{id}` | `HarvestDetailDto` (entries with hive names) |
| POST | `/harvests` | `201 + HarvestDetailDto` |
| PUT | `/harvests/{id}` | `200 + HarvestDetailDto` (apiary immutable — not in body) |
| DELETE | `/harvests/{id}` | `204` |
| GET | `/harvests/hive/{beehiveId}/yield` | `HiveYieldDto` `{ currentSeasonKg, byYear:[{year, kg}] }` |

**Create body:** `{ apiaryId, date, honeyType, pricePerKg?, notes?, entries:[{beehiveId, quantityKg, framesExtracted?}] }`
**Access:** apiary-scoped (like apiary management); managers write, **Beekeeper read-only** for harvests
containing an assigned hive. Foreign/duplicate hive in `entries` → `400`.
`GET /api/stats` gains: `seasonTotalKg`, `estimatedRevenue`, `kgByApiary[]`, `kgByHoneyType[]`,
`topHivesByYield[]`, `yearlyYield[]`.

---

### Treatments (Evidencija tretmana)

| Method | Path | Returns |
|---|---|---|
| GET | `/treatments?apiaryId=&beehiveId=&year=` | `TreatmentDto[]` (role-scoped; incl. computed `karencaUntil`, `status`/`statusName`, `hiveCount`, `hiveNames`) |
| GET | `/treatments/{id}` | `TreatmentDetailDto` (entries with hive names + `doseNote`, `createdByName`) |
| POST | `/treatments` | `201 + TreatmentDetailDto` |
| PUT | `/treatments/{id}` | `200 + TreatmentDetailDto` (apiary immutable — not in body) |
| DELETE | `/treatments/{id}` | `204` |

**Create body:** `{ apiaryId, purpose, productName, activeSubstance, method, dosePerHive, startDate,
endDate?, withdrawalDays, batchNumber?, supplier?, notes?, entries:[{beehiveId, doseNote?}] }`
**Access:** apiary-scoped (same matrix as Harvests); managers write, **Beekeeper read-only** for
treatments containing an assigned hive. Foreign/duplicate hive in `entries` → `400`; `entries` non-empty;
`withdrawalDays` 0–365. Status derived: no `endDate` → U toku; `today ≤ karencaUntil` → Karenca; else Završen.
The PDF register is generated client-side (jsPDF) — no PDF endpoint.

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

### AI Advisor

| Method | Path | Returns |
|---|---|---|
| GET | `/advisor/conversations` | `AdvisorConversationSummary[]` (own only, newest activity first) |
| GET | `/advisor/conversations/{id}` | `AdvisorConversationDetail` (with messages; 404 if not owner) |
| POST | `/advisor/conversations` | `201 + AdvisorConversationDetail` — `{ beehiveId?, message }`; `ai-chat` 10/min |
| POST | `/advisor/conversations/{id}/messages` | `200 + { userMessage, assistantMessage }`; `ai-chat` 10/min |
| POST | `/advisor/transcribe` | `{ transcript }` — multipart audio, 15 MB cap, `voice-parse` policy |
| DELETE | `/advisor/conversations/{id}` | `204` (owner only) |

**Grounding:** when `beehiveId` is set, answers are grounded in that hive's data (access checked via
`IAccessGuard`). Message length 1–4000; 60 messages/conversation cap → `422`. AI outage → `422` with a
Bosnian message and **nothing persisted**. Reuses `Groq:ApiKey`.

---

## Enum Reference

```
BeehiveType:     Langstroth | DadantBlatt | Warré | TopBar | Other
BeehiveMaterial: Wood | Plastic | Polystyrene
HoneyType:       Acacia | Linden | Chestnut | Sunflower | Meadow | Forest | Rapeseed | Other  (BsLabels: Bagrem, Lipa, …)
NotificationType: … | InspectionOverdue=10 | HoneyLevelDrop=11 | FrostWarning=12 | OldQueen=13 | WeeklySummary=14
HoneyLevel:      Low | Medium | High
TodoPriority:    Low | Medium | High
DietStatus:      NotStarted | InProgress | Completed | StoppedEarly
DietReason:      LackOfFood | WinterFeeding | SpringStimulation | (+ 6 more)
FoodType:        SugarSyrup | Fondant | Pollen | ProteinPatties | Custom
UserRole:        Admin | SystemAdmin
```
