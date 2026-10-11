from uuid import uuid4

from fastapi.testclient import TestClient

from tests.conftest import read_fixture


def _csrf(client: TestClient) -> dict[str, str]:
    token = client.cookies.get("bugshield_csrf") or ""
    return {"X-CSRF-Token": token}


def test_setup_login_and_session(client: TestClient):
    assert client.get("/api/setup/status").json()["needs_setup"] is True
    created = client.post(
        "/api/setup",
        json={
            "email": "owner@example.test",
            "password": "correct-horse-battery",
            "display_name": "Owner",
            "timezone": "America/New_York",
        },
    )
    assert created.status_code == 200
    assert created.json()["email"] == "owner@example.test"
    assert client.cookies.get("bugshield_session")
    me = client.get("/api/auth/me")
    assert me.status_code == 200
    assert me.json()["timezone"] == "America/New_York"
    conflict = client.post(
        "/api/setup",
        json={
            "email": "other@example.test",
            "password": "correct-horse-battery",
            "display_name": "Other",
        },
    )
    assert conflict.status_code == 409


def test_csrf_required_for_mutating_calls(admin_client: TestClient):
    admin_client.headers.pop("X-CSRF-Token", None)
    response = admin_client.post("/api/sites", json={"name": "No CSRF"})
    assert response.status_code == 403


def test_site_tokens_shown_once_and_ingest(admin_client: TestClient):
    site = admin_client.post("/api/sites", json={"name": "Shop", "hostname": "shop.example.test"}).json()
    token = admin_client.post(f"/api/sites/{site['id']}/tokens", json={"label": "edge"}).json()
    assert token["token"].startswith("bs_")
    listed = admin_client.get(f"/api/sites/{site['id']}/tokens").json()
    assert "token" not in listed[0]
    assert listed[0]["token_prefix"] == token["token"][:12]

    ingest = admin_client.post(
        "/api/ingest",
        content=read_fixture("json_v3_blocked.json"),
        headers={"Authorization": f"Bearer {token['token']}"},
    )
    assert ingest.status_code == 200
    assert ingest.json()["accepted"] == 1

    again = admin_client.post(
        "/api/ingest",
        content=read_fixture("json_v3_blocked.json"),
        headers={"Authorization": f"Bearer {token['token']}"},
    )
    assert again.json()["duplicates"] == 1
    assert again.json()["accepted"] == 0


def test_cross_site_access_denied(admin_client: TestClient, client: TestClient):
    site = admin_client.post("/api/sites", json={"name": "Private"}).json()
    token = admin_client.post(f"/api/sites/{site['id']}/tokens", json={"label": "a"}).json()
    admin_client.post(
        "/api/ingest",
        content=read_fixture("json_v3_blocked.json"),
        headers={"Authorization": f"Bearer {token['token']}"},
    )
    events = admin_client.get("/api/events").json()["items"]
    event_id = events[0]["id"]

    # Second user is not created in the MVP (single admin). Unknown site IDs 404.
    missing = admin_client.get(f"/api/sites/{uuid4()}")
    assert missing.status_code == 404
    missing_events = admin_client.get(f"/api/events?site_id={uuid4()}")
    assert missing_events.status_code == 404
    detail = admin_client.get(f"/api/events/{event_id}")
    assert detail.status_code == 200
    assert "[REDACTED]" in detail.json()["source_record_redacted"]
    assert "secret-token" not in detail.json()["source_record_redacted"]


def test_search_dashboard_and_demo(admin_client: TestClient):
    loaded = admin_client.post("/api/settings/demo")
    assert loaded.status_code == 200
    assert loaded.json()["accepted"] >= 5
    dashboard = admin_client.get("/api/dashboard").json()
    assert dashboard["event_count"] >= 5
    assert dashboard["disrupted_count"] >= 1
    assert dashboard["detected_count"] >= 1
    assert dashboard["demo_count"] >= 1
    assert dashboard["top_rules"]
    filtered = admin_client.get("/api/events", params={"rule_id": "942100", "severity": "critical"})
    assert filtered.status_code == 200
    assert filtered.json()["total"] >= 1
    first = filtered.json()["items"][0]
    detail = admin_client.get(f"/api/events/{first['id']}").json()
    assert "not invent" not in detail["outcome_note"].lower()
    assert detail["rules"]


def test_change_password(admin_client: TestClient, client: TestClient):
    denied = admin_client.post(
        "/api/auth/password",
        json={"current_password": "wrong-password", "new_password": "new-correct-horse"},
    )
    assert denied.status_code == 401
    changed = admin_client.post(
        "/api/auth/password",
        json={"current_password": "correct-horse-battery", "new_password": "new-correct-horse"},
    )
    assert changed.status_code == 200
    admin_client.post("/api/auth/logout")
    failed = client.post(
        "/api/auth/login",
        json={"email": "admin@example.test", "password": "correct-horse-battery"},
    )
    assert failed.status_code == 401
    ok = client.post(
        "/api/auth/login",
        json={"email": "admin@example.test", "password": "new-correct-horse"},
    )
    assert ok.status_code == 200


def test_health(client: TestClient):
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json()["database"] == "ok"
