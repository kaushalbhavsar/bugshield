# Reference analysis of the original BugShield project

This document records what could be learned about the discontinued BugShield
product and which behaviors this successor reuses. The original Windows tree at
`D:\Kaushal\Projects\bugshield` was not available in the environment that built
this repository. No source, credentials, or secrets were copied from it.

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
| ASP.NET + Bootstrap | Admin UI, sites, reports | FastAPI + React/TypeScript. |

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
