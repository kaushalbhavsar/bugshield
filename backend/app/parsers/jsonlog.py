from __future__ import annotations

import json
from datetime import datetime, timezone
from typing import Any

from app.parsers.models import ParseIssue, ParsedEvent, ParsedRule
from app.parsers.native import parse_modsec_timestamp
from app.parsers.rules import normalize_severity, parse_modsec_message, severity_from_rules


def parse_json_record(record: str) -> tuple[list[ParsedEvent], list[ParseIssue]]:
    issues: list[ParseIssue] = []
    try:
        payload = json.loads(record)
    except json.JSONDecodeError as exc:
        return [], [ParseIssue(f"invalid JSON: {exc.msg}", offset=exc.pos)]

    documents = payload if isinstance(payload, list) else [payload]
    events: list[ParsedEvent] = []
    for document in documents:
        if not isinstance(document, dict):
            issues.append(ParseIssue("JSON entry was not an object"))
            continue
        event, extra = _parse_document(document, record)
        issues.extend(extra)
        if event:
            events.append(event)
    return events, issues


def _parse_document(document: dict[str, Any], raw: str) -> tuple[ParsedEvent | None, list[ParseIssue]]:
    if "transaction" in document and isinstance(document["transaction"], dict):
        txn = document["transaction"]
        if any(key in txn for key in ("client_ip", "unique_id", "time_stamp", "request")):
            return _parse_v3(document, raw)
        return _parse_v2(document, raw)
    if "request" in document or "audit_data" in document:
        return _parse_v2(document, raw)
    return None, [ParseIssue("JSON object did not match a known ModSecurity audit schema")]


def _parse_v3(document: dict[str, Any], raw: str) -> tuple[ParsedEvent | None, list[ParseIssue]]:
    txn = document["transaction"]
    request = txn.get("request") if isinstance(txn.get("request"), dict) else {}
    response = txn.get("response") if isinstance(txn.get("response"), dict) else {}
    producer = txn.get("producer") if isinstance(txn.get("producer"), dict) else {}
    headers = _as_str_dict(request.get("headers"))
    rules = _rules_from_v3_messages(txn.get("messages"))
    occurred = _parse_time(txn.get("time_stamp") or txn.get("time"))
    if occurred is None:
        return None, [ParseIssue("JSON v3 transaction is missing a usable time_stamp")]
    unique_id = str(txn.get("unique_id") or "")
    engine_mode = str(producer.get("secrules_engine") or "").lower() or "unknown"
    event = ParsedEvent(
        unique_id=unique_id,
        occurred_at=occurred,
        source_ip=_optional_ip(txn.get("client_ip")),
        source_port=_optional_int(txn.get("client_port")),
        dest_ip=_optional_ip(txn.get("host_ip")),
        dest_port=_optional_int(txn.get("host_port")),
        method=str(request.get("method") or ""),
        uri=str(request.get("uri") or ""),
        host=str(headers.get("Host") or headers.get("host") or ""),
        status_code=_optional_int(response.get("http_code") or response.get("status")),
        action=_action_from_messages(txn.get("messages"), engine_mode),
        engine_mode=engine_mode,
        severity=severity_from_rules(rules),
        producer=_producer_text(producer),
        log_format="json-v3",
        request_headers=headers,
        messages=_v3_message_texts(txn.get("messages")),
        rules=rules,
        source_record=raw if raw.strip().startswith("{") else json.dumps(document),
    )
    return event, []


def _parse_v2(document: dict[str, Any], raw: str) -> tuple[ParsedEvent | None, list[ParseIssue]]:
    txn = document.get("transaction") if isinstance(document.get("transaction"), dict) else {}
    request = document.get("request") if isinstance(document.get("request"), dict) else {}
    response = document.get("response") if isinstance(document.get("response"), dict) else {}
    audit = document.get("audit_data") if isinstance(document.get("audit_data"), dict) else {}
    headers = _as_str_dict(request.get("headers"))
    method, uri = _split_request_line(request.get("request_line"), request)
    occurred = _parse_time(txn.get("time") or txn.get("time_stamp"))
    if occurred is None:
        return None, [ParseIssue("JSON v2 transaction is missing a usable time")]
    rules = _rules_from_v2_messages(audit.get("messages"))
    engine_mode = str(audit.get("engine_mode") or "").strip("\"'").lower() or "unknown"
    event = ParsedEvent(
        unique_id=str(txn.get("transaction_id") or txn.get("unique_id") or ""),
        occurred_at=occurred,
        source_ip=_optional_ip(txn.get("remote_address") or txn.get("client_ip")),
        source_port=_optional_int(txn.get("remote_port") or txn.get("client_port")),
        dest_ip=_optional_ip(txn.get("local_address") or txn.get("host_ip")),
        dest_port=_optional_int(txn.get("local_port") or txn.get("host_port")),
        method=method,
        uri=uri,
        host=str(headers.get("Host") or headers.get("host") or ""),
        status_code=_optional_int(
            response.get("status") or response.get("http_code") or _status_from_line(response.get("protocol"))
        ),
        action=str(audit.get("action") or "unknown"),
        engine_mode=engine_mode,
        severity=severity_from_rules(rules),
        producer=_join_producer(audit.get("producer")),
        log_format="json-v2",
        request_headers=headers,
        messages=_as_str_list(audit.get("messages")),
        rules=rules,
        source_record=raw if raw.strip().startswith("{") else json.dumps(document),
    )
    return event, []


def _rules_from_v3_messages(messages: Any) -> list[ParsedRule]:
    rules: list[ParsedRule] = []
    if not isinstance(messages, list):
        return rules
    for item in messages:
        if isinstance(item, str):
            parsed = parse_modsec_message(item)
            if parsed:
                rules.append(parsed)
            continue
        if not isinstance(item, dict):
            continue
        details = item.get("details") if isinstance(item.get("details"), dict) else {}
        rules.append(
            ParsedRule(
                rule_id=str(details.get("ruleId") or details.get("id") or ""),
                message=str(item.get("message") or details.get("data") or ""),
                action="",
                severity=normalize_severity(details.get("severity")),
                tags=_as_str_list(details.get("tags")),
                file=str(details.get("file") or ""),
                line_number=_optional_int(details.get("lineNumber") or details.get("line")),
                raw=json.dumps(item),
            )
        )
    return rules


def _rules_from_v2_messages(messages: Any) -> list[ParsedRule]:
    rules: list[ParsedRule] = []
    for message in _as_str_list(messages):
        parsed = parse_modsec_message(message)
        if parsed:
            rules.append(parsed)
        elif message:
            rules.append(ParsedRule(message=message, raw=message))
    return rules


def _v3_message_texts(messages: Any) -> list[str]:
    texts: list[str] = []
    if not isinstance(messages, list):
        return texts
    for item in messages:
        if isinstance(item, str):
            texts.append(item)
        elif isinstance(item, dict) and item.get("message"):
            texts.append(str(item["message"]))
    return texts


def _action_from_messages(messages: Any, engine_mode: str) -> str:
    if isinstance(messages, list):
        for item in messages:
            if isinstance(item, dict):
                details = item.get("details") if isinstance(item.get("details"), dict) else {}
                for key in ("action", "match"):
                    value = str(details.get(key) or "").lower()
                    if "deny" in value or "drop" in value or "block" in value:
                        return value
    return "detect" if engine_mode in {"detectiononly", "detection_only", "detection-only"} else "unknown"


def _parse_time(value: Any) -> datetime | None:
    if value is None:
        return None
    if isinstance(value, (int, float)):
        return datetime.fromtimestamp(float(value), tz=timezone.utc)
    return parse_modsec_timestamp(str(value))


def _optional_ip(value: Any) -> str | None:
    text = str(value or "").strip()
    return text or None


def _optional_int(value: Any) -> int | None:
    if value is None or value == "":
        return None
    try:
        return int(value)
    except (TypeError, ValueError):
        return None


def _as_str_dict(value: Any) -> dict[str, str]:
    if not isinstance(value, dict):
        return {}
    return {str(key): "" if val is None else str(val) for key, val in value.items()}


def _as_str_list(value: Any) -> list[str]:
    if value is None:
        return []
    if isinstance(value, list):
        return [item if isinstance(item, str) else json.dumps(item) for item in value]
    return [str(value)]


def _split_request_line(request_line: Any, request: dict[str, Any]) -> tuple[str, str]:
    if request.get("method") or request.get("uri"):
        return str(request.get("method") or ""), str(request.get("uri") or "")
    text = str(request_line or "")
    parts = text.split()
    if len(parts) >= 2:
        return parts[0], parts[1]
    return "", text


def _status_from_line(value: Any) -> int | None:
    text = str(value or "")
    parts = text.split()
    for part in parts:
        if part.isdigit() and len(part) == 3:
            return int(part)
    return None


def _producer_text(producer: dict[str, Any]) -> str:
    parts = [
        str(producer.get("modsecurity") or ""),
        str(producer.get("connector") or ""),
    ]
    components = producer.get("components")
    if isinstance(components, list):
        parts.extend(str(item) for item in components)
    return " | ".join(part for part in parts if part)


def _join_producer(value: Any) -> str:
    if isinstance(value, list):
        return " | ".join(str(item) for item in value)
    return str(value or "")
