from __future__ import annotations

import json
import re
from typing import Any

DEFAULT_HEADER_NAMES = {
    "authorization",
    "proxy-authorization",
    "cookie",
    "set-cookie",
    "x-api-key",
    "x-auth-token",
    "x-csrf-token",
}
DEFAULT_FIELD_NAMES = {
    "password",
    "passwd",
    "pwd",
    "secret",
    "token",
    "access_token",
    "refresh_token",
    "id_token",
    "api_key",
    "apikey",
    "authorization",
    "cookie",
    "credit_card",
    "cardnumber",
    "cvv",
    "ssn",
}
REDACTED = "[REDACTED]"
QUERY_PAIR = re.compile(r"(?P<key>[^&=]+)=(?P<value>[^&]*)")
HEADER_LINE = re.compile(
    r"^(?P<name>Authorization|Proxy-Authorization|Cookie|Set-Cookie|X-Api-Key|X-Auth-Token)\s*:\s*.+$",
    re.IGNORECASE | re.MULTILINE,
)


def redact_event_payload(
    headers: dict[str, str],
    uri: str,
    source_record: str,
    extra_headers: list[str] | None = None,
    extra_fields: list[str] | None = None,
) -> tuple[dict[str, str], str, str]:
    header_names = {name.lower() for name in DEFAULT_HEADER_NAMES}
    field_names = {name.lower() for name in DEFAULT_FIELD_NAMES}
    header_names.update(name.lower() for name in extra_headers or [])
    field_names.update(name.lower() for name in extra_fields or [])

    redacted_headers = {
        key: (REDACTED if key.lower() in header_names else value) for key, value in headers.items()
    }
    redacted_uri = redact_query(uri, field_names)
    redacted_source = redact_source_record(source_record, header_names, field_names)
    return redacted_headers, redacted_uri, redacted_source


def redact_query(uri: str, field_names: set[str]) -> str:
    if "?" not in uri:
        return uri
    path, query = uri.split("?", 1)
    fragment = ""
    if "#" in query:
        query, fragment = query.split("#", 1)
        fragment = "#" + fragment

    def replace(match: re.Match[str]) -> str:
        key = match.group("key")
        if key.lower() in field_names:
            return f"{key}={REDACTED}"
        return match.group(0)

    return f"{path}?{QUERY_PAIR.sub(replace, query)}{fragment}"


def redact_source_record(source: str, header_names: set[str], field_names: set[str]) -> str:
    text = HEADER_LINE.sub(lambda match: f"{match.group('name')}: {REDACTED}", source)
    try:
        payload = json.loads(source)
    except json.JSONDecodeError:
        return redact_query(text, field_names)
    return json.dumps(_redact_json(payload, header_names, field_names), ensure_ascii=True)


def _redact_json(value: Any, header_names: set[str], field_names: set[str]) -> Any:
    if isinstance(value, dict):
        redacted: dict[str, Any] = {}
        for key, item in value.items():
            lowered = str(key).lower()
            if lowered in header_names or lowered in field_names:
                redacted[key] = REDACTED
            elif lowered == "headers" and isinstance(item, dict):
                redacted[key] = {
                    header: (REDACTED if str(header).lower() in header_names else inner)
                    for header, inner in item.items()
                }
            elif lowered == "uri" and isinstance(item, str):
                redacted[key] = redact_query(item, field_names)
            else:
                redacted[key] = _redact_json(item, header_names, field_names)
        return redacted
    if isinstance(value, list):
        return [_redact_json(item, header_names, field_names) for item in value]
    if isinstance(value, str):
        return redact_query(value, field_names)
    return value
