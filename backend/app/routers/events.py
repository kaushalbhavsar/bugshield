from datetime import datetime
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, Query, status
from sqlalchemy.orm import Session

from app.db import get_db
from app.deps import current_user
from app.models import User
from app.schemas import EventDetail, EventPage, EventRuleOut, EventSummary
from app.services.events import OUTCOME_NOTES, get_event, query_events, user_site_ids
from app.timeutil import as_utc

router = APIRouter(prefix="/api/events", tags=["events"])


def _summary(event) -> EventSummary:
    return EventSummary(
        id=event.id,
        site_id=event.site_id,
        occurred_at=event.occurred_at,
        source_ip=str(event.source_ip) if event.source_ip else None,
        method=event.method,
        uri=event.uri,
        host=event.host,
        status_code=event.status_code,
        action=event.action,
        outcome=event.outcome,
        severity=event.severity,
        rule_ids=[rule.rule_id for rule in event.rules if rule.rule_id],
        is_demo=event.is_demo,
    )


@router.get("", response_model=EventPage)
def list_events(
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
    site_id: UUID | None = None,
    occurred_from: datetime | None = None,
    occurred_to: datetime | None = None,
    source_ip: str | None = None,
    url: str | None = None,
    rule_id: str | None = None,
    severity: str | None = None,
    outcome: str | None = None,
    page: int = Query(default=1, ge=1),
    page_size: int = Query(default=25, ge=1, le=100),
) -> EventPage:
    site_ids = user_site_ids(db, user.id, site_id)
    if site_id and not site_ids:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="site not found")
    items, total = query_events(
        db,
        site_ids,
        as_utc(occurred_from),
        as_utc(occurred_to),
        source_ip,
        url,
        rule_id,
        severity,
        outcome,
        page,
        page_size,
    )
    return EventPage(items=[_summary(item) for item in items], total=total, page=page, page_size=page_size)


@router.get("/{event_id}", response_model=EventDetail)
def event_detail(
    event_id: UUID,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> EventDetail:
    site_ids = user_site_ids(db, user.id)
    event = get_event(db, site_ids, event_id)
    if event is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="event not found")
    summary = _summary(event)
    return EventDetail(
        **summary.model_dump(),
        unique_id=event.unique_id,
        dest_ip=str(event.dest_ip) if event.dest_ip else None,
        dest_port=event.dest_port,
        source_port=event.source_port,
        engine_mode=event.engine_mode,
        producer=event.producer,
        log_format=event.log_format,
        ingested_at=event.ingested_at,
        request_headers_redacted=event.request_headers_redacted,
        messages=event.messages,
        rules=[
            EventRuleOut(
                rule_id=rule.rule_id,
                message=rule.message,
                action=rule.action,
                severity=rule.severity,
                tags=list(rule.tags or []),
                file=rule.file,
                line_number=rule.line_number,
            )
            for rule in event.rules
        ],
        source_record_redacted=event.source_record_redacted,
        outcome_note=OUTCOME_NOTES.get(event.outcome, ""),
    )
