from fastapi import APIRouter, Depends, HTTPException, Response, status
from sqlalchemy.orm import Session

from app.db import get_db
from app.models import User
from app.schemas import SetupRequest, SetupStatus, UserOut
from app.security import hash_password, new_token, session_expiry, set_session_cookies
from app.models import Session as AuthSession
from app.security import hash_token

router = APIRouter(prefix="/api/setup", tags=["setup"])


def needs_setup(db: Session) -> bool:
    return db.query(User).count() == 0


@router.get("/status", response_model=SetupStatus)
def setup_status(db: Session = Depends(get_db)) -> SetupStatus:
    return SetupStatus(needs_setup=needs_setup(db))


@router.post("", response_model=UserOut)
def create_admin(payload: SetupRequest, response: Response, db: Session = Depends(get_db)) -> User:
    if not needs_setup(db):
        raise HTTPException(status_code=status.HTTP_409_CONFLICT, detail="administrator already exists")
    user = User(
        email=payload.email.lower(),
        password_hash=hash_password(payload.password),
        display_name=payload.display_name,
        timezone=payload.timezone or "UTC",
    )
    db.add(user)
    db.flush()
    session_token = new_token()
    csrf_secret = new_token()
    db.add(
        AuthSession(
            user_id=user.id,
            token_hash=hash_token(session_token),
            csrf_secret=csrf_secret,
            expires_at=session_expiry(),
        )
    )
    db.commit()
    set_session_cookies(response, session_token, csrf_secret)
    return user
