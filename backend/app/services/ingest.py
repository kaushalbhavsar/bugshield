from __future__ import annotations

from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from app.config import get_settings
from app.models import Event, EventRule, Site
from app.outcomes import classify_outcome
from app.parsers import parse_payload
from app.parsers.models import ParsedEvent
from app.redaction import redact_event_payload
from app.schemas import IngestResponse
from app.settings_store import get_app_settings


def ingest_payload(
    db: Session,
    site: Site,
    payload: str,
    log_format: str | None = None,
    is_demo: bool = False,
) -> IngestResponse:
    settings = get_settings()
    if len(payload.encode("utf-8", errors="replace")) > settings.max_upload_bytes:
        raise ValueError("payload exceeds the configured upload size limit")

    parsed = parse_payload(payload, log_format)
    if len(parsed.events) > settings.max_events_per_request:
        raise ValueError(f"at most {settings.max_events_per_request} events can be ingested at once")

    app_settings = get_app_settings(db)
    extra_headers = [str(item) for item in app_settings.get("extra_redact_headers") or []]
    extra_fields = [str(item) for item in app_settings.get("extra_redact_fields") or []]

    accepted = 0
    duplicates = 0
    rejected = 0
    errors = [issue.message for issue in parsed.issues]

    for event in parsed.events:
        raw_size = len(event.source_record.encode("utf-8", errors="replace"))
        if raw_size > settings.max_event_bytes:
            rejected += 1
            errors.append(f"event {event.unique_id or event.unique_key} exceeds the event size limit")
            continue
        created, duplicate = persist_event(db, site, event, extra_headers, extra_fields, is_demo)
        if duplicate:
            duplicates += 1
        elif created:
            accepted += 1
        else:
            rejected += 1

    db.commit()
    return IngestResponse(
        accepted=accepted,
        duplicates=duplicates,
        rejected=rejected,
        incomplete_tail=parsed.incomplete_tail,
        detected_format=parsed.detected_format,
        errors=errors[:50],
    )


def persist_event(
    db: Session,
    site: Site,
    event: ParsedEvent,
    extra_headers: list[str],
    extra_fields: list[str],
    is_demo: bool,
) -> tuple[bool, bool]:
    existing = (
        db.query(Event)
        .filter(Event.site_id == site.id, Event.unique_key == event.unique_key)
        .one_or_none()
    )
    if existing:
        return False, True

    headers, uri, source = redact_event_payload(
        event.request_headers,
        event.uri,
        event.source_record,
        extra_headers,
        extra_fields,
    )
    outcome, action = classify_outcome(event)
    row = Event(
        site_id=site.id,
        unique_key=event.unique_key,
        unique_id=event.unique_id,
        occurred_at=event.occurred_at,
        source_ip=event.source_ip,
        source_port=event.source_port,
        dest_ip=event.dest_ip,
        dest_port=event.dest_port,
        method=event.method,
        uri=uri,
        host=event.host,
        status_code=event.status_code,
        action=action,
        outcome=outcome,
        engine_mode=event.engine_mode or "unknown",
        severity=event.severity,
        producer=event.producer,
        log_format=event.log_format,
        is_demo=is_demo,
        request_headers_redacted=headers,
        messages=event.messages,
        source_record_redacted=source,
    )
    try:
        with db.begin_nested():
            db.add(row)
            db.flush()
    except IntegrityError:
        return False, True

    for rule in event.rules:
        db.add(
            EventRule(
                event_id=row.id,
                rule_id=rule.rule_id,
                message=rule.message,
                action=rule.action,
                severity=rule.severity,
                tags=rule.tags,
                file=rule.file,
                line_number=rule.line_number,
            )
        )
    return True, False
