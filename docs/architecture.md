# Architecture

BugShield is a self-hosted collector and console for ModSecurity audit logs.
ModSecurity remains the component that detects and, when configured to do so,
applies disruptive actions. This application never executes request content,
never fetches URLs found in logs, and never pushes WAF rule changes.

```
[nginx/httpd + ModSecurity] --audit log--> [agent or upload]
                                              |
                                              v
                                         FastAPI API
                                              |
                                         PostgreSQL
                                              |
                                         React UI
```

## Components

| Component | Role |
| --- | --- |
| `backend/app` | FastAPI application: auth, sites, ingest, search, dashboard, settings |
| `backend/app/parsers` | Structured parsers for JSON v3, JSON v2, and native serial logs |
| `backend/app/agent.py` | File tailer with rotation, retries, and persisted offsets |
| `frontend` | Operator UI |
| PostgreSQL | Users, sessions, sites, hashed tokens, events, rule rows, settings |

## Data flow

1. An administrator creates a site and a one-time ingestion token.
2. The agent tails `SecAuditLog` (or an operator uploads a file).
3. Only complete records are sent. The agent keeps the leftover tail unacked.
4. The API parses, redacts, classifies, and inserts events in one transaction.
5. A unique key (`modsec:<unique_id>` or a content hash) prevents duplicates
   when a retry resends the same record.
6. The UI reads aggregates and event details. Rule explanations are the
   recorded `msg`, `id`, tags, and action fields — nothing invented.

## Detected vs disrupted

`outcome` is derived only from evidence in the record:

- `disrupted` — a disruptive action (`Intercepted`, `deny`, `drop`, `block`)
  and an engine mode that is not DetectionOnly.
- `detected` — rules matched, but a block was not confirmed. This includes
  DetectionOnly mode.
- `observed` — an audit record without matched-rule evidence.

HTTP 403 alone is not treated as a confirmed block.

## Security boundaries

- Administrator passwords are hashed with Argon2.
- Ingestion tokens are SHA-256 hashed. The secret is returned once.
- Sessions are random tokens in HTTP-only cookies. Mutating browser calls
  require a CSRF header that matches a non-HttpOnly double-submit cookie.
- Every site-scoped query checks that the current user owns the site.
- Cookies, Authorization, and common credential fields are redacted before
  storage. Extra field names are configurable.
- Upload and event sizes are bounded. Ingest is rate-limited.

## Tradeoffs

- **No message queue in the MVP.** The original product used RabbitMQ. A
  synchronous ingest API plus a retrying agent is enough for moderate audit
  volume and keeps the dependency list small.
- **SQL migration files instead of Alembic.** The schema is small; versioned
  SQL is easier to review.
- **Single administrator.** The schema already stores site ownership so
  additional users can be added later without rewriting tenancy checks.
- **In-process rate limits.** These reset per API process. Use a reverse
  proxy if you need a cluster-wide limit.
- **Agent lives in the backend package.** Parsers stay in one place. The
  agent can still run on a different host with the same image or venv.

## Out of scope

Billing, subscriptions, active vulnerability scanning, remote WAF rule
updates, and automatic signature generation are intentionally omitted.
