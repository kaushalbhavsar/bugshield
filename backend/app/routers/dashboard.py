from datetime import datetime
from uuid import UUID

from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session

from app.db import get_db
from app.deps import current_user
from app.models import User
from app.schemas import DashboardOut
from app.services.dashboard import build_dashboard
from app.services.events import user_site_ids
from app.timeutil import as_utc

router = APIRouter(prefix="/api/dashboard", tags=["dashboard"])


@router.get("", response_model=DashboardOut)
def dashboard(
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
    site_id: UUID | None = None,
    occurred_from: datetime | None = None,
    occurred_to: datetime | None = None,
) -> DashboardOut:
    site_ids = user_site_ids(db, user.id, site_id)
    if site_id and not site_ids:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="site not found")
    return build_dashboard(db, site_ids, as_utc(occurred_from), as_utc(occurred_to))
