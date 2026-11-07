from fastapi import APIRouter, Depends, Response, status
from fastapi.responses import JSONResponse
from sqlalchemy.orm import Session

from app.core.config import get_settings
from app.core.security import create_session_token
from app.database import get_db
from app.dependencies.auth import get_current_user
from app.models import ApplicationUser
from app.schemas.auth import CurrentUserResponse, LoginRequest, RegisterRequest
from app.services.auth import RegistrationError, authenticate, register

router = APIRouter(prefix="/api/Auth", tags=["authentication"])


def _set_session_cookie(response: Response, token: str) -> None:
    settings = get_settings()
    response.set_cookie(
        key=settings.session_cookie_name,
        value=token,
        httponly=True,
        secure=settings.secure_cookies,
        samesite="none" if settings.secure_cookies else "lax",
        path="/",
    )


def _clear_session_cookie(response: Response) -> None:
    settings = get_settings()
    response.delete_cookie(
        key=settings.session_cookie_name,
        httponly=True,
        secure=settings.secure_cookies,
        samesite="none" if settings.secure_cookies else "lax",
        path="/",
    )


@router.post("/register", status_code=status.HTTP_201_CREATED)
def register_account(request: RegisterRequest, db: Session = Depends(get_db)):
    try:
        register(db, request)
    except RegistrationError as exc:
        if exc.conflict:
            return JSONResponse(status_code=409, content={"message": str(exc)})
        return JSONResponse(
            status_code=400,
            content={"message": str(exc), "errors": exc.errors},
        )
    return {"message": "Account and business created successfully."}


@router.post("/login")
def login(
    request: LoginRequest,
    response: Response,
    db: Session = Depends(get_db),
):
    user = authenticate(db, str(request.email), request.password)
    if user is None or user.user_name is None or user.email is None:
        return JSONResponse(status_code=401, content={"message": "Invalid email or password."})
    token = create_session_token(user.id, user.email, user.user_name)
    _set_session_cookie(response, token)
    return {"message": "Login successful."}


@router.get("/me", response_model=CurrentUserResponse)
def me(user: ApplicationUser = Depends(get_current_user)) -> dict[str, str]:
    return {
        "user_id": user.id,
        "business_name": user.user_name or "",
        "email": user.email or "",
    }


@router.post("/logout")
def logout(response: Response, user: ApplicationUser = Depends(get_current_user)) -> dict[str, str]:
    _clear_session_cookie(response)
    return {"message": "Logged out successfully."}