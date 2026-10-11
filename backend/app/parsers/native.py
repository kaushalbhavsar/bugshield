from __future__ import annotations

import re
from datetime import datetime, timedelta, timezone

from app.parsers.detect import NATIVE_BOUNDARY
from app.parsers.models import ParseIssue, ParsedEvent, ParsedRule
from app.parsers.rules import parse_modsec_message, severity_from_rules

SECTION_A = re.compile(
    r"^\[(?P<ts>[^\]]+)\]\s+"
    r"(?P<unique_id>\S+)\s+"
    r"(?P<src_ip>\S+)\s+"
    r"(?P<src_port>\d+)\s+"
    r"(?P<dst_ip>\S+)\s+"
    r"(?P<dst_port>\d+)\s*$"
)
REQUEST_LINE = re.compile(
    r"^(?P<method>[A-Z]+)\s+(?P<uri>\S+)(?:\s+(?P<proto>HTTP/\S+))?\s*$"
)
HEADER_LINE = re.compile(r"^(?P<name>[^:\s]+):\s*(?P<value>.*)$")
H_FIELD = re.compile(r"^(?P<key>[A-Za-z0-9_-]+):\s*(?P<value>.*)$")


def parse_native_record(record: str) -> tuple[ParsedEvent | None, list[ParseIssue]]:
    parts = _split_parts(record)
    issues: list[ParseIssue] = []
    if "A" not in parts or "Z" not in parts:
        return None, [ParseIssue("native entry is missing required parts A and Z")]

    header = _parse_section_a(parts["A"])
    if header is None:
        return None, [ParseIssue("native section A did not match the documented header layout")]

    request = _parse_section_b(parts.get("B", ""))
    status_code = _parse_status(parts.get("F", ""))
    trailer = _parse_section_h(parts.get("H", ""))
    rules = list(trailer["rules"])
    rules.extend(_parse_section_k(parts.get("K", "")))
    if not rules and trailer["messages"]:
        for message in trailer["messages"]:
            parsed = parse_modsec_message(message)
            if parsed:
                rules.append(parsed)

    action = trailer["action"] or "unknown"
    engine_mode = trailer["engine_mode"] or "unknown"
    event = ParsedEvent(
        unique_id=header["unique_id"],
        occurred_at=header["occurred_at"],
        source_ip=header["source_ip"],
        source_port=header["source_port"],
        dest_ip=header["dest_ip"],
        dest_port=header["dest_port"],
        method=request["method"],
        uri=request["uri"],
        host=request["headers"].get("Host", ""),
        status_code=status_code,
        action=action,
        engine_mode=engine_mode,
        severity=severity_from_rules(rules),
        producer=trailer["producer"],
        log_format="native",
        request_headers=request["headers"],
        messages=trailer["messages"],
        rules=rules,
        source_record=record,
    )
    return event, issues


def _split_parts(record: str) -> dict[str, str]:
    parts: dict[str, list[str]] = {}
    current: str | None = None
    for line in record.splitlines():
        match = NATIVE_BOUNDARY.match(line)
        if match:
            current = match.group(2)
            parts.setdefault(current, [])
            continue
        if current:
            parts[current].append(line)
    return {key: "\n".join(value).strip("\n") for key, value in parts.items()}


def _parse_section_a(body: str) -> dict | None:
    line = body.strip().splitlines()[0] if body.strip() else ""
    match = SECTION_A.match(line)
    if not match:
        return None
    occurred = parse_modsec_timestamp(match.group("ts"))
    if occurred is None:
        return None
    return {
        "occurred_at": occurred,
        "unique_id": match.group("unique_id"),
        "source_ip": match.group("src_ip"),
        "source_port": int(match.group("src_port")),
        "dest_ip": match.group("dst_ip"),
        "dest_port": int(match.group("dst_port")),
    }


def _parse_section_b(body: str) -> dict:
    lines = [line for line in body.splitlines() if line.strip() != ""]
    method = ""
    uri = ""
    headers: dict[str, str] = {}
    if not lines:
        return {"method": method, "uri": uri, "headers": headers}
    request = REQUEST_LINE.match(lines[0])
    if request:
        method = request.group("method")
        uri = request.group("uri")
        header_lines = lines[1:]
    else:
        header_lines = lines
    for line in header_lines:
        header = HEADER_LINE.match(line)
        if header:
            headers[header.group("name")] = header.group("value")
    return {"method": method, "uri": uri, "headers": headers}


def _parse_status(body: str) -> int | None:
    for line in body.splitlines():
        match = re.match(r"^HTTP/\S+\s+(\d{3})", line)
        if match:
            return int(match.group(1))
    return None


def _parse_section_h(body: str) -> dict:
    messages: list[str] = []
    rules: list[ParsedRule] = []
    action = ""
    engine_mode = ""
    producer = ""
    for line in body.splitlines():
        field = H_FIELD.match(line)
        if not field:
            continue
        key = field.group("key")
        value = field.group("value").strip()
        if key in {"Message", "Apache-Error"}:
            messages.append(value)
            parsed = parse_modsec_message(value)
            if parsed:
                rules.append(parsed)
        elif key == "Action":
            action = value.strip("\"'")
        elif key == "Engine-Mode":
            engine_mode = value.strip("\"'").lower()
        elif key == "Producer":
            producer = value
    return {
        "messages": messages,
        "rules": rules,
        "action": action,
        "engine_mode": engine_mode,
        "producer": producer,
    }


def _parse_section_k(body: str) -> list[ParsedRule]:
    rules: list[ParsedRule] = []
    for line in body.splitlines():
        stripped = line.strip()
        if not stripped:
            continue
        parsed = parse_modsec_message(stripped)
        if parsed:
            rules.append(parsed)
        else:
            rules.append(ParsedRule(message=stripped, raw=stripped))
    return rules


def parse_modsec_timestamp(value: str) -> datetime | None:
    value = value.strip()
    formats = (
        "%d/%b/%Y:%H:%M:%S %z",
        "%d/%b/%Y:%H:%M:%S",
        "%a %b %d %H:%M:%S %Y",
        "%Y-%m-%dT%H:%M:%S%z",
        "%Y-%m-%d %H:%M:%S",
    )
    for fmt in formats:
        try:
            parsed = datetime.strptime(value, fmt)
            if parsed.tzinfo is None:
                parsed = parsed.replace(tzinfo=timezone.utc)
            return parsed.astimezone(timezone.utc)
        except ValueError:
            continue
    match = re.match(
        r"(?P<dow>\w{3} )?(?P<mon>\w{3})\s+(?P<day>\d{1,2}) "
        r"(?P<hms>\d{2}:\d{2}:\d{2}) (?P<year>\d{4})",
        value,
    )
    if match:
        try:
            parsed = datetime.strptime(
                f"{match.group('mon')} {match.group('day')} {match.group('hms')} {match.group('year')}",
                "%b %d %H:%M:%S %Y",
            )
            return parsed.replace(tzinfo=timezone.utc)
        except ValueError:
            return None
    match = re.match(r"(.+?) ([+-]\d{4})$", value)
    if match:
        try:
            naive = datetime.strptime(match.group(1), "%d/%b/%Y:%H:%M:%S")
            offset = match.group(2)
            sign = 1 if offset[0] == "+" else -1
            hours = int(offset[1:3])
            minutes = int(offset[3:5])
            tz = timezone(sign * timedelta(hours=hours, minutes=minutes))
            return naive.replace(tzinfo=tz).astimezone(timezone.utc)
        except ValueError:
            return None
    return None
