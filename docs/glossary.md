# Glossary

Domain terms used in BeeHive. Use these names exactly — in code, docs, and UI labels.

---

## Domain Terms

| Term | Code Name | Definition |
|---|---|---|
| Apiary | `Apiary` | A physical location (yard/field) where beehives are kept. Called *pčelinjak* in Bosnian. |
| Beehive | `Beehive` | An individual hive box. Called *košnica* in Bosnian. Belongs to one Apiary. |
| Inspection | `Inspection` | A recorded hive check. Captures health indicators at a point in time. Called *pregled*. |
| Diet | `Diet` | A structured feeding program for one beehive with a defined reason, food type, and schedule. |
| Feeding Entry | `FeedingEntry` | One scheduled feeding event within a Diet. Has a date and completion status. |
| Todo | `Todo` | A task reminder scoped to either an Apiary or a Beehive (never both). |
| Organization | `Organization` | The top-level tenant. Each organization has its own users and apiaries. |
| User | `User` | A person with access to one Organization. Has role Admin or SystemAdmin. |
| QR Code | `qrCode` | A Base64 PNG image encoding a Beehive's `uniqueId`. Used for physical hive scanning. |
| Unique ID | `uniqueId` | A Guid assigned to a Beehive at creation. Stable, never changes. Encoded in the QR code. |

---

## Role Terms

| Term | Meaning |
|---|---|
| `Admin` | Standard user. Manages apiaries, beehives, and all child entities within their organization. |
| `SystemAdmin` | Platform administrator. Manages organizations and users. Has access to `/api/admin`. |

---

## Status Terms

| Enum | Values | Domain Meaning |
|---|---|---|
| `DietStatus` | `NotStarted` | Diet created but no entries completed yet. Editable. |
| | `InProgress` | At least one feeding entry completed. No longer editable. |
| | `Completed` | All feeding entries completed. |
| | `StoppedEarly` | Manually terminated via "complete early" action with a comment. |
| `FeedingEntryStatus` | `Pending` | Scheduled but not yet done. |
| | `Completed` | Marked as done by the user. |
| `HoneyLevel` | `Low / Medium / High` | Estimated honey store level observed during an inspection. |
| `TodoPriority` | `Low / Medium / High` | Urgency of a task. |

---

## Architecture Terms

| Term | Meaning |
|---|---|
| Clean Architecture | The 4-layer backend pattern: API → Application → Domain → Infrastructure. |
| UoW | Unit of Work. The `IUnitOfWork` interface that coordinates repositories and `SaveChangesAsync()`. |
| Repository | A typed data-access class wrapping EF Core queries for one entity. |
| DTO | Data Transfer Object. Input/output contract between API and client. Never the domain entity itself. |
| Feature Slice | One folder in `Application/` containing service + DTOs + validators for one domain concept. |
| React Query | TanStack Query v5. Used for all server-state fetching, caching, and mutation in the frontend. |
| PWA | Progressive Web App. The frontend is installable and has offline caching via Workbox. |
| Problem Details | RFC 7807 JSON error format returned by the API for all error responses. |

---

## Diet Reason Terms

| Code | Human Label | When Used |
|---|---|---|
| `LackOfFood` | Lack of Food | Colony is starving or stores critically low |
| `WinterFeeding` | Winter Feeding | Pre-winter or winter supplemental feeding |
| `SpringStimulation` | Spring Stimulation | Early spring to trigger brood production |
| `SummerDearth` | Summer Dearth | Feeding during nectar gap in summer |
| `PreWinterPrep` | Pre-Winter Prep | Late summer preparation before winter |
| `ColonyReinforcement` | Colony Reinforcement | Strengthen a weak colony |
| `OrphanColony` | Orphan Colony | Colony lost its queen |
| `AfterTreatment` | After Treatment | Recovery feeding post-medication |
| `Other` | Other | Any other reason (free-text notes apply) |
