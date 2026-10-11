from datetime import datetime, timedelta, timezone

from app.models import Event, Site, User
from app.security import hash_password
from app.services.retention import purge_expired_events
from app.settings_store import put_app_setting


def test_retention_keeps_recent_and_demo(db):
    user = User(
        email="retain@example.test",
        password_hash=hash_password("correct-horse-battery"),
        display_name="R",
        timezone="UTC",
    )
    db.add(user)
    db.flush()
    site = Site(owner_id=user.id, name="R", slug="retain", hostname="", notes="")
    db.add(site)
    db.flush()
    old = Event(
        site_id=site.id,
        unique_key="old",
        unique_id="old",
        occurred_at=datetime.now(timezone.utc) - timedelta(days=200),
        method="GET",
        uri="/old",
        log_format="json-v3",
        is_demo=False,
        request_headers_redacted={},
        messages=[],
    )
    demo = Event(
        site_id=site.id,
        unique_key="demo",
        unique_id="demo",
        occurred_at=datetime.now(timezone.utc) - timedelta(days=200),
        method="GET",
        uri="/demo",
        log_format="json-v3",
        is_demo=True,
        request_headers_redacted={},
        messages=[],
    )
    recent = Event(
        site_id=site.id,
        unique_key="recent",
        unique_id="recent",
        occurred_at=datetime.now(timezone.utc),
        method="GET",
        uri="/new",
        log_format="json-v3",
        is_demo=False,
        request_headers_redacted={},
        messages=[],
    )
    db.add_all([old, demo, recent])
    put_app_setting(db, "retention_days", 90)
    db.commit()
    assert purge_expired_events(db) == 1
    keys = {row.unique_key for row in db.query(Event).all()}
    assert keys == {"demo", "recent"}
