from __future__ import annotations

from datetime import timedelta

from sqlalchemy.orm import Session

from app.models import Event
from app.security import utcnow
from app.settings_store import get_app_settings


def purge_expired_events(db: Session) -> int:
    settings = get_app_settings(db)
    days = int(settings.get("retention_days") or 90)
    cutoff = utcnow() - timedelta(days=days)
    rows = db.query(Event).filter(Event.occurred_at < cutoff, Event.is_demo.is_(False)).all()
    count = len(rows)
    for row in rows:
        db.delete(row)
    db.commit()
    return count
