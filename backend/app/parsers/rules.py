from __future__ import annotations

import re

from app.parsers.models import ParsedRule

BRACKET_FIELD = re.compile(r'\[(?P<key>[a-zA-Z0-9_-]+)\s+"(?P<value>(?:\\.|[^"\\])*)"\]')
SEVERITY_RANK = {
    "emergency": 8,
    "alert": 7,
    "critical": 6,
    "error": 5,
    "warning": 4,
    "notice": 3,
    "info": 2,
    "debug": 1,
    "unknown": 0,
}
NUMERIC_SEVERITY = {
    "0": "emergency",
    "1": "alert",
    "2": "critical",
    "3": "error",
    "4": "warning",
    "5": "notice",
    "6": "info",
    "7": "debug",
}


def parse_modsec_message(message: str) -> ParsedRule | None:
    fields: dict[str, list[str]] = {}
    for match in BRACKET_FIELD.finditer(message):
        fields.setdefault(match.group("key").lower(), []).append(match.group("value"))
    if not fields and "id" not in message and "msg" not in message:
        return None
    tags = fields.get("tag", [])
    severity = normalize_severity(fields.get("severity", [""])[0] if fields.get("severity") else "")
    rule = ParsedRule(
        rule_id=(fields.get("id") or [""])[0],
        message=(fields.get("msg") or [""])[0] or _leading_message(message),
        action="",
        severity=severity,
        tags=tags,
        file=(fields.get("file") or [""])[0],
        line_number=_as_int((fields.get("line") or [None])[0]),
        raw=message,
    )
    if not rule.rule_id and not rule.message:
        return None
    return rule


def _leading_message(message: str) -> str:
    cut = message.find(" [")
    return message[:cut].strip() if cut > 0 else message.strip()


def _as_int(value: str | None) -> int | None:
    if value is None or value == "":
        return None
    try:
        return int(value)
    except ValueError:
        return None


def normalize_severity(value: str | int | None) -> str:
    if value is None or value == "":
        return "unknown"
    text = str(value).strip().lower()
    if text in SEVERITY_RANK:
        return text
    if text in NUMERIC_SEVERITY:
        return NUMERIC_SEVERITY[text]
    return "unknown"


def severity_from_rules(rules: list[ParsedRule]) -> str:
    best = "unknown"
    best_rank = -1
    for rule in rules:
        rank = SEVERITY_RANK.get(normalize_severity(rule.severity), 0)
        if rank > best_rank:
            best = normalize_severity(rule.severity)
            best_rank = rank
    return best
