from __future__ import annotations

import os
from collections.abc import Generator
from pathlib import Path

import pytest
from fastapi.testclient import TestClient
from sqlalchemy import create_engine, text
from sqlalchemy.orm import sessionmaker

os.environ.setdefault(
    "DATABASE_URL",
    "postgresql+psycopg://bugshield:bugshield@127.0.0.1:5432/bugshield_test",
)

from app.config import get_settings
from app.db import get_db
from app.main import app
from app.migrate import apply_migrations

FIXTURES = Path(__file__).parent / "fixtures"
TABLES = [
    "event_rules",
    "events",
    "agent_checkpoints",
    "site_tokens",
    "sites",
    "sessions",
    "users",
    "app_settings",
]


@pytest.fixture(scope="session")
def _migrated_db() -> None:
    get_settings.cache_clear()
    engine = create_engine(os.environ["DATABASE_URL"], pool_pre_ping=True)
    with engine.begin() as conn:
        conn.execute(text("DROP SCHEMA public CASCADE"))
        conn.execute(text("CREATE SCHEMA public"))
    apply_migrations()


@pytest.fixture(autouse=True)
def _clean_tables(_migrated_db) -> None:
    engine = create_engine(os.environ["DATABASE_URL"], pool_pre_ping=True)
    with engine.begin() as conn:
        conn.execute(text("TRUNCATE " + ", ".join(TABLES) + " CASCADE"))


@pytest.fixture()
def db(_migrated_db):
    engine = create_engine(os.environ["DATABASE_URL"], pool_pre_ping=True)
    Session = sessionmaker(bind=engine)
    session = Session()
    try:
        yield session
    finally:
        session.close()


@pytest.fixture()
def client(_migrated_db) -> Generator[TestClient, None, None]:
    engine = create_engine(os.environ["DATABASE_URL"], pool_pre_ping=True)
    TestingSession = sessionmaker(bind=engine)

    def override_db():
        session = TestingSession()
        try:
            yield session
        finally:
            session.close()

    app.dependency_overrides[get_db] = override_db
    with TestClient(app) as test_client:
        yield test_client
    app.dependency_overrides.clear()


@pytest.fixture()
def admin_client(client: TestClient) -> TestClient:
    response = client.post(
        "/api/setup",
        json={
            "email": "admin@example.test",
            "password": "correct-horse-battery",
            "display_name": "Admin",
            "timezone": "UTC",
        },
    )
    assert response.status_code == 200
    client.headers["X-CSRF-Token"] = client.cookies.get("bugshield_csrf") or ""
    return client


def read_fixture(name: str) -> str:
    return (FIXTURES / name).read_text(encoding="utf-8")
