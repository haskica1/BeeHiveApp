# SPEC-09 — Plans & Billing ("Paketi i naplata")

| | |
|---|---|
| **Status** | 📋 Planned |
| **Effort** | M/L (~2 days) |
| **Depends on** | nothing (gates SPEC-01 AI features when it lands) |
| **New secrets / packages** | none in v1 (Phase 2 payment provider adds a webhook secret) |

## Goal

Monetize the platform with per-organization subscription plans: a **Free** plan that keeps small
hobby beekeepers on board and a **Pro** plan that unlocks the costly features (Groq AI) and lifts
scale limits. **v1 billing is manual** (bank transfer / invoice; SystemAdmin activates the plan in
the admin UI) — Stripe does not support BiH-based merchant accounts, so the automated-payments
phase targets a merchant-of-record provider (Paddle) later. v1 ships the whole plan model,
enforcement, and upgrade UX; only the payment click-flow is deferred.

## Plans (v1)

| | **Free** | **Pro** |
|---|---|---|
| Pčelinjaci | 2 | neograničeno |
| Košnice | 10 | neograničeno |
| AI Savjetnik (chat + glas) | ✗ | ✓ |
| Glasovni unos pregleda | ✗ | ✓ |
| Sedmični AI sažetak | ✗ | ✓ |
| Rule-based alarmi, evidencije (tretmani, vrcanja…), PDF registri | ✓ | ✓ |

Limits live in config (`Plans:Free:MaxApiaries` = 2, `Plans:Free:MaxBeehives` = 10) — never
hardcoded in services. **Data entry and legal artifacts are never paywalled** (the treatment PDF
register is a legal obligation); the paywall covers AI (real Groq cost) and scale.

## Domain rules

- Plan is **per organization**: `Organization` += `Plan PlanType { Free=1, Pro=2 }` (default Free),
  `PlanValidUntil DateTime?` (null = bez isteka), `PlanNotes string(300)?` (manual bookkeeping:
  invoice number, ko je platio…). Migration `AddOrganizationPlan`.
- **Effective plan is computed, not stored** (Diets/Treatments precedent —
  `PlanHelper.Effective(plan, validUntil, today)`): `Pro` + `PlanValidUntil < today` → behaves as
  **Free**. No background job flips anything.
- Downgrade/expiry never touches data: existing hives/apiaries above the limit stay readable and
  editable — limits are enforced **on create only**. AI endpoints stop working immediately.
- SystemAdmin organizations (none — SystemAdmin has no org) and the org-less SystemAdmin user are
  unaffected by gates.

## Backend

### Enforcement — `IPlanGuard` (single source, `IAccessGuard` precedent)

```
IPlanGuard
  EnsureCanAddApiaryAsync(organizationId)    // called by ApiaryService.CreateAsync
  EnsureCanAddBeehiveAsync(organizationId)   // called by BeehiveService.CreateAsync
  EnsureAiAvailableAsync(organizationId)     // advisor create/send, voice parse (inspections + advisor transcribe)
  GetEffectivePlanAsync(organizationId)      // → PlanType (for DTOs/UI)
```

Violations throw `PlanLimitException(message)` → GlobalExceptionMiddleware maps to **402 Payment
Required** with ProblemDetails `{ code: "plan-limit", detail: <bosanska poruka> }` — distinct from
403 so the frontend can render the upsell instead of "nemate pravo". Messages: *"Free paket
uključuje do {n} košnica — nadogradite na Pro."* / *"AI funkcije su dio Pro paketa."*

`WeeklySummaryService`: skip Free organizations (no Groq call, log-and-continue).

### Endpoints

| Method | Path | Notes |
|---|---|---|
| GET | `/api/organizations/my-plan` | current org's `{ plan, planName, effectivePlan, planValidUntil, apiaryCount/limit, beehiveCount/limit }` — any authenticated user |
| PUT | `/api/admin/organizations/{id}/plan` | SystemAdmin: `{ plan, planValidUntil?, planNotes? }` → updated org (manual activation v1) |

Existing `AdminService` org DTOs gain `plan`/`planValidUntil` fields (admin list shows who pays).

### Alert (SPEC-04 rule table)

`PlanExpiring` (NotificationType = **18** — 15/16 SPEC-08, 17 SPEC-06): `PlanValidUntil` within
next 7 days → notify the org's OrganizationAdmins *"Pro paket ističe {date} — produžite da
zadržite AI funkcije."*; dedup 7 days, toggle `Alerts:PlanExpiring:Enabled`.

## Frontend

- Models: `PlanType` enum + labels ("Besplatni", "Pro"), `MyPlan` model; `planService.ts` + hooks.
- **`/plans` page** (`features/plans/PlansPage.tsx`, all authenticated users): two plan cards
  (features table above, price from a single `PLAN_PRICING` const — informational v1), current-plan
  badge, usage meters (košnice {used}/{limit}), CTA **"Kontaktirajte nas za nadogradnju"**
  (mailto + upute za uplatu) — no payment flow in v1.
- **Upsell handling**: axios interceptor recognises 402 + `code: "plan-limit"` → global
  `UpsellModal` (poruka sa servera + link na `/plans`). Voice-input and advisor entry points also
  hide/disable proactively when `myPlan.effectivePlan === Free` (upsell hint instead of a dead
  button) — the 402 path stays as the backstop.
- **Admin**: `OrganizationFormPage` (SystemAdmin) gains plan select + `planValidUntil` date +
  `planNotes`; admin org table shows the plan column.
- Org admins see plan status + expiry on `/plans`; nav stays unchanged (no new nav item — link from
  profile dropdown "Paket: {name}").

## Phase 2 — automated payments (separate follow-up, out of v1)

Merchant-of-record provider (**Paddle** — available to BiH-based sellers, handles EU VAT) →
checkout link on `/plans` → webhook `POST /api/billing/webhook` (new secret) sets
`Plan/PlanValidUntil`. The v1 seam is exactly these two fields — no schema change expected.

## Edge cases

- Org downgraded with 30 hives: everything stays visible/editable; creating hive #31 → 402.
- `PlanValidUntil` today → still Pro through the end of the day (compare dates, not instants).
- Free org opens `/advisor` with an old conversation: history is readable (data never locked);
  sending a message → 402 upsell.
- SystemAdmin sets Pro without expiry → lifetime Pro (early adopters / partner orgs).
- Registration keeps creating Free orgs — no behavior change for new users.

## Out of scope (v1)

Online payment flow (Phase 2 — Paddle), per-seat pricing, more than two tiers, trials/coupons,
usage-based AI metering, invoicing PDF generation, plan change history/audit table.

## Acceptance criteria

- [ ] Free org: creating the 3rd apiary / 11th hive → 402 with `code: "plan-limit"` and Bosnian
      message; Pro org unlimited (unit tests on `PlanGuard` with config overrides).
- [ ] Free org: advisor create/send + voice parse → 402; weekly summary worker skips the org
      (unit test: Groq client never called).
- [ ] Expired Pro (`PlanValidUntil` in the past) behaves exactly as Free (helper unit tests incl.
      same-day boundary).
- [ ] Downgrade locks nothing: existing over-limit data stays readable/editable (test: update on
      hive #15 of a Free org succeeds).
- [ ] SystemAdmin plan update endpoint role-guarded (403 for others) and reflected in
      `/organizations/my-plan`.
- [ ] Frontend: 402 anywhere → UpsellModal with the server message + link to `/plans`; `/plans`
      shows correct usage meters; AI entry points disabled with hint for Free orgs.
- [ ] `PlanExpiring` alert fires once (dedup) within 7 days of expiry, only to OrgAdmins.
- [ ] All labels Bosnian (`BsLabels` + frontend maps). Docs updated: `features/plans-billing.md`,
      `api-contracts.md`, `context.md`, `decisions.md` (manual-billing-first + Paddle-over-Stripe
      rationale), this spec → ✅.
