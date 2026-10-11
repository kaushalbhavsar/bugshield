from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    app_name: str = "BugShield"
    app_version: str = "0.1.0"
    database_url: str = "postgresql+psycopg://bugshield:bugshield@127.0.0.1:5432/bugshield"
    session_ttl_hours: int = 12
    cookie_secure: bool = False
    cookie_samesite: str = "lax"
    cors_origins: str = "http://localhost:5173,http://127.0.0.1:5173,http://localhost:8080"
    max_upload_bytes: int = 10 * 1024 * 1024
    max_event_bytes: int = 512 * 1024
    max_events_per_request: int = 200
    ingest_rate_limit_per_minute: int = 120
    default_retention_days: int = 90
    default_timezone: str = "UTC"

    @property
    def cors_origin_list(self) -> list[str]:
        return [item.strip() for item in self.cors_origins.split(",") if item.strip()]


@lru_cache
def get_settings() -> Settings:
    return Settings()
