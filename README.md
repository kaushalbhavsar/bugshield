# BugShield

Self-hosted dashboard for ModSecurity audit logs.

ModSecurity performs detection and blocking. BugShield collects those audit
records, searches them, and shows what the log actually recorded. It does
**not** claim that every event was blocked.

This repository is a modern open-source successor to the discontinued
BugShield SecaaS product (ASP.NET / MySQL). The original Windows tree was not
included here and no credentials were copied from it. See
[docs/legacy-reference.md](docs/legacy-reference.md).

## What the MVP does

- First-run administrator setup, session cookies, and CSRF protection
- Site management with per-site ingestion tokens (shown once, hashed at rest)
- Ingest via authenticated API or file upload
- Parsers for ModSecurity JSON (v2 and v3) and native serial audit logs
- File-tailing agent with rotation handling, retries, and persisted offsets
- Dashboard: event counts, confirmed disruptive actions, trends, top rules,
  source IPs, and URLs
- Search and event detail with redacted source records
- Configurable retention and extra redaction fields
- Labelled synthetic demo dataset

Out of scope: billing, subscriptions, vulnerability scanning, and remote WAF
rule changes.

## Quick start (local)

PostgreSQL 16+ and Python 3.12+ are required. Node 22+ is required for the UI.

```bash
sudo service postgresql start
sudo -u postgres psql -c "CREATE USER bugshield WITH PASSWORD 'bugshield';"
sudo -u postgres psql -c "CREATE DATABASE bugshield OWNER bugshield;"

cd backend
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
export DATABASE_URL=postgresql+psycopg://bugshield:bugshield@127.0.0.1:5432/bugshield
python -m app.migrate
uvicorn app.main:app --host 127.0.0.1 --port 8000
```

```bash
cd frontend
npm install
npm run dev
```

Open http://127.0.0.1:5173. Create the administrator, add a site from
Preferences, then either upload `backend/tests/fixtures/` samples or load the
demo dataset from Preferences.

Health check: `curl http://127.0.0.1:8000/health`

## Docker Compose

```bash
docker compose up --build
```

UI: http://127.0.0.1:8080  
API: http://127.0.0.1:8000/docs

Set `COOKIE_SECURE=true` behind HTTPS.

## Agent

```bash
cd backend
python -m app.agent --config ../deploy/agent.example.json
```

See [docs/modsecurity-setup.md](docs/modsecurity-setup.md).

## Documentation

- [Architecture](docs/architecture.md)
- [Parser compatibility](docs/compatibility.md)
- [ModSecurity setup](docs/modsecurity-setup.md)
- [API](docs/api.md)
- [Contributing](docs/contributing.md)
- [Example environment](.env.example)

## Tests

```bash
cd backend
source .venv/bin/activate
export DATABASE_URL=postgresql+psycopg://bugshield:bugshield@127.0.0.1:5432/bugshield_test
pytest
```

Tests use synthetic fixtures only.

## License

Apache License 2.0. See [LICENSE](LICENSE).
