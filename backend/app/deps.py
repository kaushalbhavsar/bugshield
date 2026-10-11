from __future__ import annotations

from collections.abc import Callable
from uuid import UUID

from fastapi import Cookie, Depends, Header, HTTPException, Request, status
from sqlalchemy.orm import Session

from app.db import get_db
from app.models import Session as AuthSession
from app.models import Site, SiteToken, User
from app.security import (
    CSRF_COOKIE,
    CSRF_HEADER,
    SESSION_COOKIE,
    csrf_matches,
    hash_token,
    utcnow,
)

SAFE_METHODS = {"GET", "HEAD", "OPTIONS"}


def current_user(
    request: Request,
    db: Session = Depends(get_db),
    session_token: str | None = Cookie(default=None, alias=SESSION_COOKIE),
    csrf_cookie: str | None = Cookie(default=None, alias=CSRF_COOKIE),
    csrf_header: str | None = Header(default=None, alias=CSRF_HEADER),
) -> User:
    if not session_token:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="authentication required")
    row = (
        db.query(AuthSession)
        .filter(AuthSession.token_hash == hash_token(session_token))
        .one_or_none()
    )
    if row is None or row.expires_at <= utcnow():
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="session expired")
    if request.method not in SAFE_METHODS and not csrf_matches(csrf_cookie, csrf_header, row.csrf_secret):
        raise HTTPException(status_code=status.HTTP_403_FORBIDDEN, detail="csrf validation failed")
    user = db.get(User, row.user_id)
    if user is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="authentication required")
    request.state.auth_session = row
    return user


def require_site(user: User, db: Session, site_id: UUID) -> Site:
    site = db.get(Site, site_id)
    if site is None or site.owner_id != user.id:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="site not found")
    return site


def site_from_token(
    db: Session = Depends(get_db),
    authorization: str | None = Header(default=None),
) -> tuple[Site, SiteToken]:
    if not authorization or not authorization.lower().startswith("bearer "):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="ingestion token required")
    token = authorization.split(" ", 1)[1].strip()
    row = (
        db.query(SiteToken)
        .filter(SiteToken.token_hash == hash_token(token), SiteToken.revoked_at.is_(None))
        .one_or_none()
    )
    if row is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="invalid ingestion token")
    site = db.get(Site, row.site_id)
    if site is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="invalid ingestion token")
    row.last_used_at = utcnow()
    return site, row


def optional_current_user(
    request: Request,
    db: Session = Depends(get_db),
    session_token: str | None = Cookie(default=None, alias=SESSION_COOKIE),
) -> User | None:
    if not session_token:
        return None
    try:
        return current_user(request, db, session_token, None, None)
    except HTTPException:
        return None


RequireUser = Callable[..., User]
