# API

Interactive OpenAPI is available at `/docs` when the API is running.

All timestamps in responses are UTC (`TIMESTAMPTZ`). The UI converts them to
the administrator's stored timezone.

## Public

| Method | Path | Notes |
| --- | --- | --- |
| GET | `/health` or `/api/health` | Liveness plus database check |
| GET | `/api/setup/status` | Whether the first admin must be created |
| POST | `/api/setup` | Create the first administrator and session |
| POST | `/api/auth/login` | Create a session cookie |

## Session cookie

After setup or login the API sets:

- `bugshield_session` — HTTP-only session token
- `bugshield_csrf` — readable CSRF secret

Browser clients must send `X-CSRF-Token: <csrf cookie>` on POST/PATCH/PUT/DELETE.

## Authenticated operator API

Requires the session cookie. Site IDs that the user does not own return 404.

| Method | Path | Purpose |
| --- | --- | --- |
| POST | `/api/auth/logout` | Delete sessions |
| GET | `/api/auth/me` | Current user |
| PATCH | `/api/auth/me` | Update timezone |
| POST | `/api/auth/password` | Change the current user's password |
| GET/POST | `/api/sites` | List / create sites |
| GET/PATCH/DELETE | `/api/sites/{id}` | Site management |
| GET/POST | `/api/sites/{id}/tokens` | List / create tokens (secret shown once) |
| DELETE | `/api/sites/{id}/tokens/{tid}` | Revoke token |
| POST | `/api/sites/{id}/uploads` | Multipart file upload |
| GET | `/api/dashboard` | Counts, trends, top rules/IPs/URLs |
| GET | `/api/events` | Search with pagination |
| GET | `/api/events/{id}` | Event detail |
| GET/PUT | `/api/settings` | Retention and extra redaction fields |
| POST | `/api/settings/retention/run` | Purge expired events |
| POST/DELETE | `/api/settings/demo` | Load or remove the demo dataset |

### Event filters

`site_id`, `occurred_from`, `occurred_to`, `source_ip`, `url`, `rule_id`,
`severity`, `outcome`, `page`, `page_size`.

Naive datetimes are interpreted as UTC.

## Ingestion API

```
POST /api/ingest
Authorization: Bearer <site token>
Content-Type: text/plain
```

Body is a JSON object, JSON array, NDJSON stream, or native serial log.

Response:

```json
{
  "accepted": 2,
  "duplicates": 1,
  "rejected": 0,
  "incomplete_tail": false,
  "detected_format": "json",
  "errors": []
}
```

Duplicates return HTTP 200 with `duplicates > 0`. The agent can treat that as
success and advance its offset.

## Errors

Validation and parser problems return HTTP 400 with `detail`.
Authentication failures return 401. CSRF failures return 403.
Unknown or unauthorized sites and events return 404.
Oversized uploads return 413. Ingest rate limits return 429.
