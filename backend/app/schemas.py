from __future__ import annotations

from datetime import datetime
from typing import Any
from uuid import UUID

from pydantic import BaseModel, Field, field_validator


class SetupStatus(BaseModel):
    needs_setup: bool


class SetupRequest(BaseModel):
    email: str = Field(min_length=3, max_length=320)
    password: str = Field(min_length=12, max_length=128)
    display_name: str = Field(min_length=1, max_length=120)
    timezone: str = Field(default="UTC", max_length=64)

    @field_validator("email")
    @classmethod
    def email_has_at(cls, value: str) -> str:
        if "@" not in value or value.startswith("@") or value.endswith("@"):
            raise ValueError("email must look like a valid address")
        return value.lower()


class LoginRequest(BaseModel):
    email: str = Field(min_length=3, max_length=320)
    password: str = Field(min_length=1, max_length=128)

    @field_validator("email")
    @classmethod
    def email_has_at(cls, value: str) -> str:
        return value.lower()


class UserOut(BaseModel):
    id: UUID
    email: str
    display_name: str
    timezone: str

    model_config = {"from_attributes": True}


class TimezoneUpdate(BaseModel):
    timezone: str = Field(min_length=1, max_length=64)


class SiteCreate(BaseModel):
    name: str = Field(min_length=1, max_length=160)
    hostname: str = Field(default="", max_length=255)
    notes: str = Field(default="", max_length=2000)


class SiteUpdate(BaseModel):
    name: str | None = Field(default=None, max_length=160)
    hostname: str | None = Field(default=None, max_length=255)
    notes: str | None = Field(default=None, max_length=2000)


class SiteOut(BaseModel):
    id: UUID
    name: str
    slug: str
    hostname: str
    notes: str
    created_at: datetime

    model_config = {"from_attributes": True}


class TokenCreate(BaseModel):
    label: str = Field(default="default", max_length=120)


class TokenOut(BaseModel):
    id: UUID
    label: str
    token_prefix: str
    created_at: datetime
    last_used_at: datetime | None
    revoked_at: datetime | None

    model_config = {"from_attributes": True}


class TokenCreated(TokenOut):
    token: str
    warning: str = "This token is shown once. Store it in the agent configuration now."


class IngestResponse(BaseModel):
    accepted: int
    duplicates: int
    rejected: int
    incomplete_tail: bool
    detected_format: str
    errors: list[str]


class EventRuleOut(BaseModel):
    rule_id: str
    message: str
    action: str
    severity: str
    tags: list[str]
    file: str
    line_number: int | None


class EventSummary(BaseModel):
    id: UUID
    site_id: UUID
    occurred_at: datetime
    source_ip: str | None
    method: str
    uri: str
    host: str
    status_code: int | None
    action: str
    outcome: str
    severity: str
    rule_ids: list[str]
    is_demo: bool


class EventDetail(EventSummary):
    unique_id: str
    dest_ip: str | None
    dest_port: int | None
    source_port: int | None
    engine_mode: str
    producer: str
    log_format: str
    ingested_at: datetime
    request_headers_redacted: dict[str, Any]
    messages: list[Any]
    rules: list[EventRuleOut]
    source_record_redacted: str
    outcome_note: str


class EventPage(BaseModel):
    items: list[EventSummary]
    total: int
    page: int
    page_size: int


class NamedCount(BaseModel):
    name: str
    count: int


class TrendPoint(BaseModel):
    bucket: datetime
    events: int
    disrupted: int


class DashboardOut(BaseModel):
    event_count: int
    disrupted_count: int
    detected_count: int
    observed_count: int
    demo_count: int
    trends: list[TrendPoint]
    top_rules: list[NamedCount]
    top_source_ips: list[NamedCount]
    top_urls: list[NamedCount]


class SettingsOut(BaseModel):
    retention_days: int
    extra_redact_headers: list[str]
    extra_redact_fields: list[str]


class SettingsUpdate(BaseModel):
    retention_days: int | None = Field(default=None, ge=1, le=3650)
    extra_redact_headers: list[str] | None = None
    extra_redact_fields: list[str] | None = None


class HealthOut(BaseModel):
    status: str
    version: str
    database: str
