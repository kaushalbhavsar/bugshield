from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime
from typing import Any


@dataclass
class ParsedRule:
    rule_id: str = ""
    message: str = ""
    action: str = ""
    severity: str = ""
    tags: list[str] = field(default_factory=list)
    file: str = ""
    line_number: int | None = None
    raw: str = ""


@dataclass
class ParsedEvent:
    unique_id: str
    occurred_at: datetime
    source_ip: str | None
    source_port: int | None
    dest_ip: str | None
    dest_port: int | None
    method: str
    uri: str
    host: str
    status_code: int | None
    action: str
    engine_mode: str
    severity: str
    producer: str
    log_format: str
    request_headers: dict[str, str]
    messages: list[str]
    rules: list[ParsedRule]
    source_record: str
    unique_key: str = ""


@dataclass
class ParseIssue:
    message: str
    offset: int | None = None
    recoverable: bool = True


@dataclass
class ParseResult:
    events: list[ParsedEvent] = field(default_factory=list)
    issues: list[ParseIssue] = field(default_factory=list)
    incomplete_tail: bool = False
    detected_format: str = "unknown"
    consumed_complete_records: int = 0
