# Feature Specs

> One spec per planned feature. Workflow: **read the spec → implement exactly what it says → tick the
> acceptance criteria → update the spec status + project docs** (per `../workflow.md`).
> A spec is the single source of truth for its feature until it ships; after shipping, the feature
> gets a `docs/features/<name>.md` file and the spec is marked ✅.

## How to implement a spec

1. Read the spec top to bottom. Read `../context.md` and `../ignore.md` first (house rule).
2. Follow the templates in `../common-patterns.md` — the spec defines *what*, the patterns define *how*.
3. Implement in the phase order given in the spec. Phases are independently shippable.
4. Tick every box in the spec's **Acceptance criteria** before considering it done.
5. Update docs per `../workflow.md` step 5 (feature file, `api-contracts.md`, `context.md`, …)
   and set the spec status to ✅ Implemented (add the date).

## Index

| # | Spec | One-liner | Effort | Depends on | Status |
|---|------|-----------|--------|------------|--------|
| 01 | [AI Advisor](SPEC-01-ai-advisor.md) | Chat savjetnik (tekst + glas) koji poznaje korisnikove košnice | M/L | — | 📋 Planned |
| 02 | [Harvest Log](SPEC-02-harvests.md) | Evidencija vrcanja: kg meda po košnici/sezoni + profitabilnost | M | — | 📋 Planned |
| 03 | [Queen Tracking](SPEC-03-queens.md) | Matice: starost, boja oznake, porijeklo, historija zamjena | S/M | — | ✅ Implemented (2026-07-02) |
| 04 | [Smart Alerts & Weekly Summary](SPEC-04-smart-alerts.md) | Automatska upozorenja (pregledi, med, mraz, matica) + sedmični AI sažetak | M | 03 (partly) | 📋 Planned |
| 05 | [Inspection Photos & AI Analysis](SPEC-05-inspection-photos.md) | Fotografije na pregledima + AI analiza okvira (vision) | L | — | 📋 Planned |
| 06 | [Learning Module](SPEC-06-learning.md) | Edukacija: sezonske teme za čitanje i slušanje | M | — | 📋 Planned |
| 07 | [Offline Inspections](SPEC-07-offline-inspections.md) | Unos pregleda bez signala — lokalni outbox + sinhronizacija | M/L | — | 📋 Planned |

**Recommended order = index order.** Rationale:

- **SPEC-01 first** — the user-facing flagship; reuses the existing Groq stack (`VoiceParsingService`
  pattern, `useVoiceInput`) so infra cost is near zero. Its hive context gets richer as 02/03 ship,
  but it works without them.
- **SPEC-02/03** are pure CRUD, independent, and feed both the advisor context and the alert rules.
- **SPEC-04** builds on the notification infra and consumes data from 02/03 (queen-age rule is
  skipped gracefully until 03 ships).
- **SPEC-05** is the largest (introduces object storage) — do not start it casually.
- **SPEC-06/07** are independent and can be slotted anywhere.

## Conventions used in the specs

- Layer layout, DTO/validator/service/controller templates: `../common-patterns.md`.
  Application code lives in `BeeHive.Application/Features/<Feature>/`, repositories + migrations in
  `BeeHive.Entity/` (persistence project), matching current code — not the older paths some docs mention.
- Authorization is **always** via `IAccessGuard` (never inline role checks in services/controllers).
- All user-facing strings (UI, notifications, AI output) are **Bosnian**; enum display labels via `BsLabels`.
- New secrets go to env vars / user-secrets, never the repo (`appsettings.json` keeps empty placeholders).
- Every schema change = one EF Core migration in `BeeHive.Entity/Migrations/`, named `Add<Thing>`.
- Statuses: 📋 Planned · 🔨 In progress · ✅ Implemented (date).
