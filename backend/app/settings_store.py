from __future__ import annotations

from typing import Any

from sqlalchemy.orm import Session

from app.config import get_settings
from app.models import AppSetting
from app.security import utcnow

DEFAULTS = {
    "retention_days": None,  # filled from config
    "extra_redact_headers": [],
    "extra_redact_fields": [],
}


def get_app_settings(db: Session) -> dict[str, Any]:
    values = {
        "retention_days": get_settings().default_retention_days,
        "extra_redact_headers": [],
        "extra_redact_fields": [],
    }
    for row in db.query(AppSetting).all():
        if isinstance(row.value, dict) and "value" in row.value:
            values[row.key] = row.value["value"]
        else:
            values[row.key] = row.value
    return values


def put_app_setting(db: Session, key: str, value: Any) -> None:
    row = db.get(AppSetting, key)
    if row is None:
        row = AppSetting(key=key, value={"value": value}, updated_at=utcnow())
        db.add(row)
    else:
        row.value = {"value": value}
        row.updated_at = utcnow()
