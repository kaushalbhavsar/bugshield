from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session

from app.db import get_db
from app.deps import current_user, require_site
from app.models import Site, SiteToken, User
from app.schemas import SiteCreate, SiteOut, SiteUpdate, TokenCreate, TokenCreated, TokenOut
from app.services.sites import create_site, create_site_token, revoke_site_token

router = APIRouter(prefix="/api/sites", tags=["sites"])


@router.get("", response_model=list[SiteOut])
def list_sites(db: Session = Depends(get_db), user: User = Depends(current_user)) -> list[Site]:
    return db.query(Site).filter(Site.owner_id == user.id).order_by(Site.name).all()


@router.post("", response_model=SiteOut, status_code=status.HTTP_201_CREATED)
def add_site(
    payload: SiteCreate,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> Site:
    site = create_site(db, user, payload.name, payload.hostname, payload.notes)
    db.commit()
    db.refresh(site)
    return site


@router.get("/{site_id}", response_model=SiteOut)
def get_site(
    site_id: UUID,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> Site:
    return require_site(user, db, site_id)


@router.patch("/{site_id}", response_model=SiteOut)
def update_site(
    site_id: UUID,
    payload: SiteUpdate,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> Site:
    site = require_site(user, db, site_id)
    if payload.name is not None:
        site.name = payload.name
    if payload.hostname is not None:
        site.hostname = payload.hostname
    if payload.notes is not None:
        site.notes = payload.notes
    db.commit()
    db.refresh(site)
    return site


@router.delete("/{site_id}")
def delete_site(
    site_id: UUID,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> dict[str, str]:
    site = require_site(user, db, site_id)
    db.delete(site)
    db.commit()
    return {"status": "ok"}


@router.get("/{site_id}/tokens", response_model=list[TokenOut])
def list_tokens(
    site_id: UUID,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> list[SiteToken]:
    site = require_site(user, db, site_id)
    return (
        db.query(SiteToken)
        .filter(SiteToken.site_id == site.id)
        .order_by(SiteToken.created_at.desc())
        .all()
    )


@router.post("/{site_id}/tokens", response_model=TokenCreated)
def add_token(
    site_id: UUID,
    payload: TokenCreate,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> TokenCreated:
    site = require_site(user, db, site_id)
    row, raw = create_site_token(db, site, payload.label)
    db.commit()
    db.refresh(row)
    return TokenCreated(
        id=row.id,
        label=row.label,
        token_prefix=row.token_prefix,
        created_at=row.created_at,
        last_used_at=row.last_used_at,
        revoked_at=row.revoked_at,
        token=raw,
    )


@router.delete("/{site_id}/tokens/{token_id}", response_model=TokenOut)
def delete_token(
    site_id: UUID,
    token_id: UUID,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> SiteToken:
    site = require_site(user, db, site_id)
    row = revoke_site_token(db, site, token_id)
    if row is None:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="token not found")
    db.commit()
    db.refresh(row)
    return row
