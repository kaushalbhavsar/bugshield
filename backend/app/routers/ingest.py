from uuid import UUID

from fastapi import APIRouter, Depends, File, HTTPException, Request, UploadFile, status
from sqlalchemy.orm import Session

from app.config import get_settings
from app.db import get_db
from app.deps import current_user, require_site, site_from_token
from app.models import Site, SiteToken, User
from app.rate_limit import ingest_rate_limit
from app.schemas import IngestResponse
from app.services.ingest import ingest_payload

router = APIRouter(tags=["ingest"])


@router.post("/api/ingest", response_model=IngestResponse)
async def ingest_api(
    request: Request,
    db: Session = Depends(get_db),
    auth: tuple[Site, SiteToken] = Depends(site_from_token),
) -> IngestResponse:
    site, token = auth
    ingest_rate_limit(request, token.token_prefix)
    payload = (await request.body()).decode("utf-8", errors="replace")
    try:
        result = ingest_payload(db, site, payload)
    except ValueError as exc:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(exc)) from exc
    return result


@router.post("/api/sites/{site_id}/uploads", response_model=IngestResponse)
async def upload_logs(
    site_id: UUID,
    request: Request,
    file: UploadFile = File(...),
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> IngestResponse:
    site = require_site(user, db, site_id)
    ingest_rate_limit(request, str(user.id))
    settings = get_settings()
    data = await file.read(settings.max_upload_bytes + 1)
    if len(data) > settings.max_upload_bytes:
        raise HTTPException(status_code=status.HTTP_413_REQUEST_ENTITY_TOO_LARGE, detail="upload exceeds size limit")
    try:
        return ingest_payload(db, site, data.decode("utf-8", errors="replace"))
    except ValueError as exc:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(exc)) from exc
