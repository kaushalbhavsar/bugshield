# Reference analysis of the original BugShield project

This document records what could be learned about the discontinued BugShield
product and which behaviors this successor reuses. The original tree is now on `main` under `old/` (`BUGSKAN.Portal`). This
successor does not modify that folder. The operator UI is ported from
`BUGSKAN.Portal` views and assets (`_Inner.cshtml`, `_Outer.cshtml`,
`Account/Login.cshtml`, `Home/Index.cshtml`, `Attacks/AttackToday.cshtml`,
`Preferences/Edit.cshtml`, `Manage/ChangePassword.cshtml`, `Home/Support.cshtml`,
plus `amaze.css`, `style.css`, Themify fonts, logos, and `bg1.jpg`). No
credentials or secrets were copied from it. Highcharts was not vendored.

## What the original product did

Public portfolio material describes BugShield as a Security-as-a-Service
platform that combined:

- A cloud vulnerability scanner (BUGSKAN lineage)
- A ModSecurity-based cloud WAF
- Live HTTP traffic monitoring
- A user-facing dashboard
- An intelligence layer ("BugSense") that generated WAF signatures from scan
  findings

The published stack was ASP.NET, Bootstrap, C++, ModSecurity, MySQL, Python,
RabbitMQ, and Redis. The product was discontinued around the end of 2018.

## Architecture implied by that stack

| Layer | Likely role | Successor approach |
| --- | --- | --- |
| ModSecurity / C++ | Detection and blocking on the edge | Unchanged: ModSecurity remains the WAF. This app never executes request content or pushes rule changes. |
| Python workers + RabbitMQ | Asynchronous log shipping / processing | Replaced by a small file-tailing agent plus a synchronous ingest API. A queue is unnecessary for the MVP volume. |
| Redis | Sessions, rate limits, or cache | Sessions live in PostgreSQL. In-process rate limits cover ingest. |
| MySQL | Primary store | PostgreSQL, with explicit migrations and UTC timestamps. |
| ASP.NET + Bootstrap | Admin UI, sites, reports | FastAPI + React/TypeScript. The operator chrome follows the original Amaze / Material Dashboard views (white sidebar, danger-red active item, rose login). |

## Reusable product behavior

These behaviors are worth keeping even without the original source:

1. **Site-centric tenancy.** Operators manage named sites and inspect events per site.
2. **Ingest then explain.** The WAF decides; the dashboard collects, searches, and reports.
3. **Operator dashboards.** Counts, trends, top rules, source IPs, and targeted URLs.
4. **Searchable event history.** Filter by time, IP, URL, rule, and severity.
5. **Per-site credentials.** Agents and uploads authenticate to one site, not the admin session.
6. **Reporting without acting.** The first version's scanner, auto-patch, billing, and remote rule updates stay out of scope.

## Behaviors that must not be copied blindly

- Do not treat every audit event as a block. DetectionOnly / `pass` / `allow` matches are detections.
- Do not invent rule explanations or remediations. Show recorded messages, IDs, tags, and actions only.
- Do not store raw cookies, Authorization headers, or credential fields.
- Do not contact URLs found in logs.

## Compatibility target for this successor

The parsers implement representative, documented formats:

- ModSecurity 3 JSON (`transaction` object with structured `messages`)
- ModSecurity 2 JSON (`transaction` / `request` / `response` / `audit_data`)
- Native serial audit logs with parts `A`–`K` and terminator `Z`

See [compatibility.md](compatibility.md) for limits.
