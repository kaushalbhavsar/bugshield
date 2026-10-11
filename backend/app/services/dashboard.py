from __future__ import annotations

from datetime import datetime
from uuid import UUID

from sqlalchemy import String, case, func
from sqlalchemy.orm import Session

from app.models import Event, EventRule
from app.schemas import DashboardOut, NamedCount, TrendPoint


def build_dashboard(
    db: Session,
    site_ids: list[UUID],
    occurred_from: datetime | None,
    occurred_to: datetime | None,
) -> DashboardOut:
    empty = DashboardOut(
        event_count=0,
        disrupted_count=0,
        detected_count=0,
        observed_count=0,
        demo_count=0,
        trends=[],
        top_rules=[],
        top_source_ips=[],
        top_urls=[],
    )
    if not site_ids:
        return empty

    filters = [Event.site_id.in_(site_ids)]
    if occurred_from:
        filters.append(Event.occurred_at >= occurred_from)
    if occurred_to:
        filters.append(Event.occurred_at <= occurred_to)

    counts = (
        db.query(
            func.count(Event.id),
            func.sum(case((Event.outcome == "disrupted", 1), else_=0)),
            func.sum(case((Event.outcome == "detected", 1), else_=0)),
            func.sum(case((Event.outcome == "observed", 1), else_=0)),
            func.sum(case((Event.is_demo.is_(True), 1), else_=0)),
        )
        .filter(*filters)
        .one()
    )
    trend_rows = (
        db.query(
            func.date_trunc("hour", Event.occurred_at).label("bucket"),
            func.count(Event.id).label("events"),
            func.sum(case((Event.outcome == "disrupted", 1), else_=0)).label("disrupted"),
        )
        .filter(*filters)
        .group_by("bucket")
        .order_by("bucket")
        .all()
    )
    top_rules = (
        db.query(EventRule.rule_id, func.count(EventRule.id))
        .join(Event, Event.id == EventRule.event_id)
        .filter(*filters)
        .filter(EventRule.rule_id != "")
        .group_by(EventRule.rule_id)
        .order_by(func.count(EventRule.id).desc())
        .limit(8)
        .all()
    )
    top_ips = (
        db.query(func.cast(Event.source_ip, String), func.count(Event.id))
        .filter(*filters)
        .filter(Event.source_ip.isnot(None))
        .group_by(Event.source_ip)
        .order_by(func.count(Event.id).desc())
        .limit(8)
        .all()
    )
    top_urls = (
        db.query(Event.uri, func.count(Event.id))
        .filter(*filters)
        .filter(Event.uri != "")
        .group_by(Event.uri)
        .order_by(func.count(Event.id).desc())
        .limit(8)
        .all()
    )
    return DashboardOut(
        event_count=int(counts[0] or 0),
        disrupted_count=int(counts[1] or 0),
        detected_count=int(counts[2] or 0),
        observed_count=int(counts[3] or 0),
        demo_count=int(counts[4] or 0),
        trends=[
            TrendPoint(bucket=row.bucket, events=row.events, disrupted=int(row.disrupted or 0))
            for row in trend_rows
        ],
        top_rules=[NamedCount(name=row[0], count=row[1]) for row in top_rules],
        top_source_ips=[NamedCount(name=row[0], count=row[1]) for row in top_ips],
        top_urls=[NamedCount(name=row[0], count=row[1]) for row in top_urls],
    )
