from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session

from app.db import get_db
from app.deps import current_user
from app.models import User
from app.schemas import SettingsOut, SettingsUpdate
from app.services.demo import clear_demo_dataset, load_demo_dataset
from app.services.events import user_site_ids
from app.services.retention import purge_expired_events
from app.services.sites import create_site
from app.settings_store import get_app_settings, put_app_setting

router = APIRouter(prefix="/api/settings", tags=["settings"])


@router.get("", response_model=SettingsOut)
def read_settings(db: Session = Depends(get_db), user: User = Depends(current_user)) -> SettingsOut:
    values = get_app_settings(db)
    return SettingsOut(
        retention_days=int(values.get("retention_days") or 90),
        extra_redact_headers=list(values.get("extra_redact_headers") or []),
        extra_redact_fields=list(values.get("extra_redact_fields") or []),
    )


@router.put("", response_model=SettingsOut)
def update_settings(
    payload: SettingsUpdate,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> SettingsOut:
    current = get_app_settings(db)
    if payload.retention_days is not None:
        put_app_setting(db, "retention_days", payload.retention_days)
        current["retention_days"] = payload.retention_days
    if payload.extra_redact_headers is not None:
        put_app_setting(db, "extra_redact_headers", payload.extra_redact_headers)
        current["extra_redact_headers"] = payload.extra_redact_headers
    if payload.extra_redact_fields is not None:
        put_app_setting(db, "extra_redact_fields", payload.extra_redact_fields)
        current["extra_redact_fields"] = payload.extra_redact_fields
    db.commit()
    return SettingsOut(
        retention_days=int(current.get("retention_days") or 90),
        extra_redact_headers=list(current.get("extra_redact_headers") or []),
        extra_redact_fields=list(current.get("extra_redact_fields") or []),
    )


@router.post("/retention/run")
def run_retention(db: Session = Depends(get_db), user: User = Depends(current_user)) -> dict[str, int]:
    return {"purged": purge_expired_events(db)}


@router.post("/demo")
def load_demo(db: Session = Depends(get_db), user: User = Depends(current_user)) -> dict[str, int]:
    from app.models import Site

    site = db.query(Site).filter(Site.owner_id == user.id, Site.slug == "demo").one_or_none()
    if site is None:
        site = create_site(db, user, "Demo site", "demo.example.test", "Synthetic evaluation data")
        db.flush()
    result = load_demo_dataset(db, site)
    return result


@router.delete("/demo")
def remove_demo(db: Session = Depends(get_db), user: User = Depends(current_user)) -> dict[str, int]:
    return {"removed": clear_demo_dataset(db, user_site_ids(db, user.id))}
