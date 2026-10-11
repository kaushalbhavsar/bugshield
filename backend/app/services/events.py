from __future__ import annotations

from datetime import datetime
from uuid import UUID

from sqlalchemy import String, func
from sqlalchemy.orm import Session, selectinload

from app.models import Event, EventRule, Site

OUTCOME_NOTES = {
    "disrupted": (
        "Recorded evidence shows a disruptive action was applied while the "
        "engine was enabled (for example Action: Intercepted, deny, drop, or block)."
    ),
    "detected": (
        "Rules matched and were logged. There is not enough evidence that a "
        "disruptive action was applied. DetectionOnly mode is treated as detection, not a block."
    ),
    "observed": (
        "This audit record did not include matched rule evidence. It is an observed "
        "transaction, not a confirmed detection or block."
    ),
}


def query_events(
    db: Session,
    site_ids: list[UUID],
    occurred_from: datetime | None = None,
    occurred_to: datetime | None = None,
    source_ip: str | None = None,
    url: str | None = None,
    rule_id: str | None = None,
    severity: str | None = None,
    outcome: str | None = None,
    page: int = 1,
    page_size: int = 25,
) -> tuple[list[Event], int]:
    filters = [Event.site_id.in_(site_ids)]
    if occurred_from:
        filters.append(Event.occurred_at >= occurred_from)
    if occurred_to:
        filters.append(Event.occurred_at <= occurred_to)
    if source_ip:
        filters.append(func.cast(Event.source_ip, String) == source_ip)
    if url:
        filters.append(Event.uri.ilike(f"%{url}%"))
    if severity:
        filters.append(Event.severity == severity.lower())
    if outcome:
        filters.append(Event.outcome == outcome)
    query = db.query(Event).options(selectinload(Event.rules)).filter(*filters)
    if rule_id:
        query = query.filter(Event.rules.any(EventRule.rule_id == rule_id))
    total = query.order_by(None).with_entities(func.count(func.distinct(Event.id))).scalar() or 0
    items = (
        db.query(Event)
        .options(selectinload(Event.rules))
        .filter(*filters)
        .filter(Event.rules.any(EventRule.rule_id == rule_id) if rule_id else True)
        .order_by(Event.occurred_at.desc())
        .offset((page - 1) * page_size)
        .limit(page_size)
        .all()
    )
    return items, total


def get_event(db: Session, site_ids: list[UUID], event_id: UUID) -> Event | None:
    return (
        db.query(Event)
        .options(selectinload(Event.rules), selectinload(Event.site))
        .filter(Event.id == event_id, Event.site_id.in_(site_ids))
        .one_or_none()
    )


def user_site_ids(db: Session, user_id: UUID, site_id: UUID | None = None) -> list[UUID]:
    query = db.query(Site.id).filter(Site.owner_id == user_id)
    if site_id:
        query = query.filter(Site.id == site_id)
    return [row[0] for row in query.all()]
