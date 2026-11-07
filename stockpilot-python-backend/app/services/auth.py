from uuid import uuid4

from sqlalchemy import func, select
from sqlalchemy.orm import Session

from app.models import ApplicationUser, Business
from app.schemas.auth import RegisterRequest
from app.core.security import hash_password, verify_password


class RegistrationError(Exception):
    def __init__(self, message: str, errors: list[str] | None = None, conflict: bool = False):
        super().__init__(message)
        self.errors = errors or []
        self.conflict = conflict


def _validate_identity_password(password: str) -> list[str]:
    errors: list[str] = []
    if not any(char.isdigit() for char in password):
        errors.append("Passwords must have at least one digit ('0'-'9').")
    if not any(char.islower() for char in password):
        errors.append("Passwords must have at least one lowercase ('a'-'z').")
    if not any(char.isupper() for char in password):
        errors.append("Passwords must have at least one uppercase ('A'-'Z').")
    if not any(not char.isalnum() for char in password):
        errors.append("Passwords must have at least one non alphanumeric character.")
    return errors


def register(db: Session, request: RegisterRequest) -> None:
    business_name = request.business_name.strip()
    email = str(request.email).strip()
    password_errors = _validate_identity_password(request.password)
    if password_errors:
        raise RegistrationError("Account creation failed.", password_errors)

    email_exists = db.scalar(
        select(ApplicationUser.id).where(
            func.upper(ApplicationUser.email) == email.upper()
        ).limit(1)
    )
    if email_exists:
        raise RegistrationError("An account with this email already exists.", conflict=True)

    # Identity normalizes usernames before applying its unique index. The C# service
    # uses the business name as UserName and separately checks exact business names.
    normalized_name = business_name.upper()
    username_exists = db.scalar(
        select(ApplicationUser.id).where(
            ApplicationUser.normalized_user_name == normalized_name
        ).limit(1)
    )
    if username_exists:
        raise RegistrationError("This business name is already taken.", conflict=True)

    business_exists = db.scalar(
        select(Business.id).where(Business.name == business_name).limit(1)
    )
    if business_exists:
        raise RegistrationError("This business name is already taken.", conflict=True)

    user = ApplicationUser(
        id=str(uuid4()),
        user_name=business_name,
        normalized_user_name=normalized_name,
        email=email,
        normalized_email=email.upper(),
        email_confirmed=False,
        password_hash=hash_password(request.password),
        security_stamp=str(uuid4()),
        concurrency_stamp=str(uuid4()),
        phone_number_confirmed=False,
        two_factor_enabled=False,
        lockout_enabled=False,
        access_failed_count=0,
    )
    business = Business(name=business_name, owner=user)
    try:
        db.add(user)
        db.add(business)
        db.commit()
    except Exception:
        db.rollback()
        raise


def authenticate(db: Session, email: str, password: str) -> ApplicationUser | None:
    normalized_email = email.strip().upper()
    user = db.scalar(
        select(ApplicationUser).where(
            ApplicationUser.normalized_email == normalized_email
        ).limit(1)
    )
    if user is None or not user.password_hash or not user.user_name:
        return None
    if not verify_password(password, user.password_hash):
        return None
    return user


def get_user_by_id(db: Session, user_id: str) -> ApplicationUser | None:
    return db.get(ApplicationUser, user_id)