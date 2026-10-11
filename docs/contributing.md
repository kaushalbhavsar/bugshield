# Contributing

Thank you for considering a contribution. This project stays intentionally
small: monitoring and reporting for ModSecurity audit logs.

## Development

```bash
sudo service postgresql start   # or use Docker Compose
createdb bugshield              # already done in docker-compose
cd backend
python3 -m venv .venv
source .venv/bin/activate
pip install -r requirements.txt
export DATABASE_URL=postgresql+psycopg://bugshield:bugshield@127.0.0.1:5432/bugshield
python -m app.migrate
pytest
uvicorn app.main:app --reload --port 8000
```

```bash
cd frontend
npm install
npm run dev
```

The Vite dev server proxies `/api` to port 8000.

## Tests that must stay green

- Parser fixtures for JSON v2/v3 and native logs
- Partial / malformed records
- Duplicate ingest
- Agent retry and rotation
- Redaction
- Authentication and CSRF
- Cross-site ID denial

Do not add tests that contact the network or production services.

## Style

- Prefer clear functions over new dependencies.
- Keep timestamps in UTC at the storage boundary.
- Never invent rule explanations.
- Never log raw ingestion tokens or passwords.

## Pull requests

Describe the behavior change, the fixtures you used, and any compatibility
limits you are introducing. Update `docs/compatibility.md` when a parser
changes.
