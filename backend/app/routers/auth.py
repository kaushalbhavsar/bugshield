from fastapi import APIRouter, Depends, HTTPException, Response, status
from sqlalchemy.orm import Session

from app.db import get_db
from app.deps import current_user
from app.models import Session as AuthSession
from app.models import User
from app.schemas import LoginRequest, TimezoneUpdate, UserOut
from app.security import (
    clear_session_cookies,
    hash_token,
    new_token,
    session_expiry,
    set_session_cookies,
    verify_password,
)

router = APIRouter(prefix="/api/auth", tags=["auth"])


@router.post("/login", response_model=UserOut)
def login(payload: LoginRequest, response: Response, db: Session = Depends(get_db)) -> User:
    user = db.query(User).filter(User.email == payload.email.lower()).one_or_none()
    if user is None or not verify_password(payload.password, user.password_hash):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="invalid email or password")
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


@router.post("/logout")
def logout(
    response: Response,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> dict[str, str]:
    db.query(AuthSession).filter(AuthSession.user_id == user.id).delete()
    db.commit()
    clear_session_cookies(response)
    return {"status": "ok"}


@router.get("/me", response_model=UserOut)
def me(user: User = Depends(current_user)) -> User:
    return user


@router.patch("/me", response_model=UserOut)
def update_me(
    payload: TimezoneUpdate,
    db: Session = Depends(get_db),
    user: User = Depends(current_user),
) -> User:
    user.timezone = payload.timezone
    db.commit()
    db.refresh(user)
    return user
