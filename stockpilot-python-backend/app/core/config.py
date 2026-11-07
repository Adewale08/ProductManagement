from functools import lru_cache

from pydantic import field_validator
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_file_encoding="utf-8", extra="ignore")

    database_url: str
    frontend_origins: str = ""
    environment: str = "development"
    session_cookie_name: str = "stockpilot_session"
    session_secret: str
    session_max_age_seconds: int = 60 * 60 * 24 * 7

    @field_validator("database_url", mode="before")
    @classmethod
    def normalize_database_url(cls, value: str) -> str:
        if "sslmode=" not in value.lower():
            value += "&sslmode=require" if "?" in value else "?sslmode=require"
        if value.startswith("postgres://"):
            return "postgresql+psycopg://" + value.removeprefix("postgres://")
        if value.startswith("postgresql://"):
            return "postgresql+psycopg://" + value.removeprefix("postgresql://")
        return value

    @field_validator("session_secret")
    @classmethod
    def validate_session_secret(cls, value: str) -> str:
        if value.strip().lower().startswith("replace-with"):
            raise ValueError("Set SESSION_SECRET to a randomly generated secret.")
        if len(value) < 32:
            raise ValueError("SESSION_SECRET must contain at least 32 characters.")
        return value

    @property
    def cors_origins(self) -> list[str]:
        return [origin.strip().rstrip("/") for origin in self.frontend_origins.split(",") if origin.strip()]

    @property
    def secure_cookies(self) -> bool:
        return self.environment.lower() not in {"development", "local", "test"}


@lru_cache
def get_settings() -> Settings:
    return Settings()
