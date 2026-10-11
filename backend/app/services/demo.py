from __future__ import annotations

from pathlib import Path

from sqlalchemy.orm import Session

from app.models import Event, Site
from app.services.ingest import ingest_payload

FIXTURES = Path(__file__).resolve().parents[2] / "tests" / "fixtures"
DEMO_FILES = [
    "json_v3_blocked.json",
    "json_v3_detection_only.json",
    "json_v2_blocked.json",
    "native_blocked.log",
    "native_detection_only.log",
    "json_v3_observed.json",
]


def load_demo_dataset(db: Session, site: Site) -> dict[str, int]:
    accepted = duplicates = rejected = 0
    for name in DEMO_FILES:
        payload = (FIXTURES / name).read_text(encoding="utf-8")
        result = ingest_payload(db, site, payload, is_demo=True)
        accepted += result.accepted
        duplicates += result.duplicates
        rejected += result.rejected
    return {"accepted": accepted, "duplicates": duplicates, "rejected": rejected}


def clear_demo_dataset(db: Session, site_ids: list) -> int:
    rows = db.query(Event).filter(Event.site_id.in_(site_ids), Event.is_demo.is_(True)).all()
    count = len(rows)
    for row in rows:
        db.delete(row)
    db.commit()
    return count
