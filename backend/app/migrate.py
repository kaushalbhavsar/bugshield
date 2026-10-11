from pathlib import Path

from sqlalchemy import text

from app.db import engine

MIGRATIONS_DIR = Path(__file__).parent / "migrations"


def _statements(sql: str) -> list[str]:
    parts: list[str] = []
    current: list[str] = []
    for line in sql.splitlines():
        current.append(line)
        if line.rstrip().endswith(";"):
            statement = "\n".join(current).strip()
            if statement:
                parts.append(statement)
            current = []
    trailing = "\n".join(current).strip()
    if trailing:
        parts.append(trailing)
    return parts


def apply_migrations() -> list[str]:
    applied: list[str] = []
    with engine.begin() as conn:
        conn.execute(
            text(
                """
                CREATE TABLE IF NOT EXISTS schema_migrations (
                    version TEXT PRIMARY KEY,
                    applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
                )
                """
            )
        )
        rows = conn.execute(text("SELECT version FROM schema_migrations")).fetchall()
        done = {row[0] for row in rows}
        for path in sorted(MIGRATIONS_DIR.glob("*.sql")):
            version = path.name
            if version in done:
                continue
            for statement in _statements(path.read_text(encoding="utf-8")):
                conn.execute(text(statement))
            conn.execute(
                text("INSERT INTO schema_migrations(version) VALUES (:version)"),
                {"version": version},
            )
            applied.append(version)
    return applied


if __name__ == "__main__":
    result = apply_migrations()
    print("applied:", ", ".join(result) if result else "none")
