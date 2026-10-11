from __future__ import annotations

import json
import re

NATIVE_BOUNDARY = re.compile(r"^--([0-9A-Za-z]+)-([A-Z])--\s*$")


def detect_format(payload: str) -> str:
    sample = payload.lstrip()
    if not sample:
        return "empty"
    if sample.startswith("{") or sample.startswith("["):
        return "json"
    head = "\n".join(sample.splitlines()[:20])
    if any(NATIVE_BOUNDARY.match(line) for line in head.splitlines()):
        return "native"
    first = sample.splitlines()[0] if sample.splitlines() else ""
    if first.startswith("{") or first.startswith("["):
        return "json"
    return "unknown"


def split_complete_records(payload: str, log_format: str | None = None) -> tuple[list[str], str]:
    """Split a payload into complete records and an incomplete tail.

    Used by both the API and the file agent so a partial last event is never
    acknowledged.
    """
    kind = log_format or detect_format(payload)
    if kind == "json":
        return _split_json_records(payload)
    if kind == "native":
        return _split_native_records(payload)
    return [], payload


def _split_json_records(payload: str) -> tuple[list[str], str]:
    stripped = payload.lstrip()
    if stripped.startswith("["):
        try:
            json.loads(stripped)
            return [stripped], ""
        except json.JSONDecodeError:
            return [], payload
    if stripped.startswith("{") and "\n{" not in stripped and not _looks_like_ndjson(payload):
        try:
            json.loads(stripped)
            return [stripped], ""
        except json.JSONDecodeError:
            return [], payload

    records: list[str] = []
    leftover_parts: list[str] = []
    for line in payload.splitlines(keepends=True):
        candidate = "".join(leftover_parts) + line
        text = candidate.strip()
        if not text:
            leftover_parts = []
            continue
        try:
            json.loads(text)
            records.append(text)
            leftover_parts = []
        except json.JSONDecodeError:
            leftover_parts.append(line)
    return records, "".join(leftover_parts)


def _looks_like_ndjson(payload: str) -> bool:
    lines = [line.strip() for line in payload.splitlines() if line.strip()]
    return len(lines) >= 2 and all(line.startswith("{") for line in lines[:3])


def _split_native_records(payload: str) -> tuple[list[str], str]:
    records: list[str] = []
    current: list[str] = []
    current_id: str | None = None
    prefix = ""
    lines = payload.splitlines(keepends=True)
    for index, line in enumerate(lines):
        match = NATIVE_BOUNDARY.match(line.rstrip("\r\n"))
        if match:
            boundary_id, part = match.groups()
            if part == "A":
                if current:
                    leftover = prefix + "".join(current) + "".join(lines[index:])
                    return records, leftover
                current = [line]
                current_id = boundary_id
                continue
            if not current:
                current = [line]
                current_id = boundary_id
            else:
                current.append(line)
            if part == "Z" and (current_id is None or current_id == boundary_id):
                records.append("".join(current))
                current = []
                current_id = None
            continue
        if current:
            current.append(line)
        else:
            prefix += line
    leftover = prefix + "".join(current)
    return records, leftover
