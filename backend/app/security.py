from __future__ import annotations

import hashlib
import hmac
import secrets
from datetime import datetime, timedelta, timezone

from argon2 import PasswordHasher
from argon2.exceptions import VerifyMismatchError
from fastapi import Response

from app.config import get_settings

hasher = PasswordHasher()
SESSION_COOKIE = "bugshield_session"
CSRF_COOKIE = "bugshield_csrf"
CSRF_HEADER = "x-csrf-token"


def hash_password(password: str) -> str:
    return hasher.hash(password)


def verify_password(password: str, password_hash: str) -> bool:
    try:
        return hasher.verify(password_hash, password)
    except VerifyMismatchError:
        return False


def new_token(prefix: str = "") -> str:
    return f"{prefix}{secrets.token_urlsafe(32)}"


def hash_token(token: str) -> str:
    return hashlib.sha256(token.encode("utf-8")).hexdigest()


def token_prefix(token: str) -> str:
    return token[:12]


def utcnow() -> datetime:
    return datetime.now(timezone.utc)


def session_expiry() -> datetime:
    return utcnow() + timedelta(hours=get_settings().session_ttl_hours)


def set_session_cookies(response: Response, session_token: str, csrf_secret: str) -> None:
    settings = get_settings()
    cookie_args = {
        "httponly": True,
        "secure": settings.cookie_secure,
        "samesite": settings.cookie_samesite,
        "path": "/",
        "max_age": settings.session_ttl_hours * 3600,
    }
    response.set_cookie(SESSION_COOKIE, session_token, **cookie_args)
    response.set_cookie(
        CSRF_COOKIE,
        csrf_secret,
        httponly=False,
        secure=settings.cookie_secure,
        samesite=settings.cookie_samesite,
        path="/",
        max_age=settings.session_ttl_hours * 3600,
    )


def clear_session_cookies(response: Response) -> None:
    response.delete_cookie(SESSION_COOKIE, path="/")
    response.delete_cookie(CSRF_COOKIE, path="/")


def csrf_matches(cookie_value: str | None, header_value: str | None, secret: str) -> bool:
    if not cookie_value or not header_value:
        return False
    return hmac.compare_digest(cookie_value, header_value) and hmac.compare_digest(cookie_value, secret)
