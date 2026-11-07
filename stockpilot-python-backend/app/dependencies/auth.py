import jwt
from fastapi import Cookie, Depends, HTTPException, status
from sqlalchemy import select
from sqlalchemy.orm import Session

from app.core.config import get_settings
from app.core.errors import APIError
from app.core.security import decode_session_token
from app.database import get_db
from app.models import ApplicationUser, Business


def get_current_user(
    token: str | None = Cookie(default=None, alias=get_settings().session_cookie_name),
    db: Session = Depends(get_db),
) -> ApplicationUser:
    if not token:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication required.")
    try:
        claims = decode_session_token(token)
    except (jwt.InvalidTokenError, ValueError, TypeError):
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication required.") from None
    user = db.get(ApplicationUser, claims.get("sub"))
    if user is None:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Authentication required.")
    return user


def get_current_business(
    user: ApplicationUser = Depends(get_current_user),
    db: Session = Depends(get_db),
) -> Business:
    business = db.scalar(
        select(Business).where(Business.owner_id == user.id).order_by(Business.id).limit(1)
    )
    if business is None:
        raise APIError(409, "The authenticated user is not associated with a business.")
    return business