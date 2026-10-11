from __future__ import annotations

import re
from uuid import UUID

from sqlalchemy.orm import Session

from app.models import Site, SiteToken, User
from app.security import hash_token, new_token, token_prefix, utcnow

SLUG_RE = re.compile(r"[^a-z0-9]+")


def slugify(name: str) -> str:
    slug = SLUG_RE.sub("-", name.lower()).strip("-")
    return slug or "site"


def unique_slug(db: Session, name: str) -> str:
    base = slugify(name)[:60]
    slug = base
    index = 2
    while db.query(Site).filter(Site.slug == slug).one_or_none():
        slug = f"{base}-{index}"
        index += 1
    return slug


def create_site(db: Session, user: User, name: str, hostname: str, notes: str) -> Site:
    site = Site(owner_id=user.id, name=name, slug=unique_slug(db, name), hostname=hostname, notes=notes)
    db.add(site)
    db.flush()
    return site


def create_site_token(db: Session, site: Site, label: str) -> tuple[SiteToken, str]:
    raw = new_token("bs_")
    row = SiteToken(
        site_id=site.id,
        token_hash=hash_token(raw),
        token_prefix=token_prefix(raw),
        label=label,
        created_at=utcnow(),
    )
    db.add(row)
    db.flush()
    return row, raw


def revoke_site_token(db: Session, site: Site, token_id: UUID) -> SiteToken | None:
    row = db.get(SiteToken, token_id)
    if row is None or row.site_id != site.id:
        return None
    row.revoked_at = utcnow()
    return row
