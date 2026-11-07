from datetime import datetime, timedelta, timezone
from uuid import uuid4

import jwt
from pwdlib import PasswordHash

from app.core.config import get_settings

_password_hash = PasswordHash.recommended()
_ALGORITHM = "HS256"
_ISSUER = "StockPilot"


def hash_password(password: str) -> str:
    return _password_hash.hash(password)


def verify_password(password: str, password_hash: str) -> bool:
    try:
        return _password_hash.verify(password, password_hash)
    except (ValueError, TypeError):
        return False


def create_session_token(user_id: str, email: str, business_name: str) -> str:
    settings = get_settings()
    now = datetime.now(timezone.utc)
    claims = {
        "sub": user_id,
        "email": email,
        "name": business_name,
        "jti": str(uuid4()),
        "iss": _ISSUER,
        "iat": now,
        "exp": now + timedelta(seconds=settings.session_max_age_seconds),
    }
    return jwt.encode(claims, settings.session_secret, algorithm=_ALGORITHM)


def decode_session_token(token: str) -> dict:
    settings = get_settings()
    return jwt.decode(
        token,
        settings.session_secret,
        algorithms=[_ALGORITHM],
        issuer=_ISSUER,
        options={"require": ["sub", "exp", "iat", "iss"]},
    )