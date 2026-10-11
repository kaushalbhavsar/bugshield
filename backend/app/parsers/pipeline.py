from __future__ import annotations

import hashlib

from app.parsers.detect import detect_format, split_complete_records
from app.parsers.jsonlog import parse_json_record
from app.parsers.models import ParseIssue, ParseResult, ParsedEvent
from app.parsers.native import parse_native_record


def parse_payload(payload: str, log_format: str | None = None) -> ParseResult:
    detected = log_format if log_format in {"json", "native"} else detect_format(payload)
    if detected == "empty":
        return ParseResult(detected_format="empty")
    if detected == "unknown":
        return ParseResult(
            issues=[ParseIssue("payload is neither ModSecurity JSON nor a native audit log")],
            detected_format="unknown",
        )

    records, leftover = split_complete_records(payload, detected)
    result = ParseResult(
        incomplete_tail=bool(leftover.strip()),
        detected_format=detected,
        consumed_complete_records=len(records),
    )
    for record in records:
        if detected == "json":
            events, issues = parse_json_record(record)
            result.issues.extend(issues)
            for event in events:
                event.unique_key = build_unique_key(event)
                result.events.append(event)
        else:
            event, issues = parse_native_record(record)
            result.issues.extend(issues)
            if event:
                event.unique_key = build_unique_key(event)
                result.events.append(event)
    return result


def build_unique_key(event: ParsedEvent) -> str:
    if event.unique_id:
        return f"modsec:{event.unique_id}"
    basis = "|".join(
        [
            event.occurred_at.isoformat(),
            event.source_ip or "",
            str(event.source_port or ""),
            event.method,
            event.uri,
            event.host,
            str(event.status_code or ""),
        ]
    )
    return "hash:" + hashlib.sha256(basis.encode("utf-8")).hexdigest()
