"""
Infrastructure Configuration - Settings
========================================

Pydantic Settings for environment-based configuration.
"""

from functools import lru_cache
from typing import Literal

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
        extra="ignore",
    )
    
    # === Application ===
    version: str = "0.6.0-alpha"
    environment: Literal["development", "production", "testing"] = "development"
    service_name: str = "python-analytics"
    log_level: str = "info"
    workers: int = 4
    
    # === Database ===
    database_url: str | None = None
    
    # === Security ===
    jwt_secret: str = ""
    internal_service_key: str = ""
    
    # === External Services ===
    parhelion_api_url: str = "http://parhelion-api:5000"
    parhelion_api_internal_key: str = ""
    
    # === CORS ===
    cors_origins: list[str] = [
        "http://localhost:4100",    # Admin
        "http://localhost:5101",    # Operaciones
        "http://localhost:5102",    # Campo
        "http://localhost:5100",    # API (.NET)
    ]
    
    @property
    def is_production(self) -> bool:
        """Check if running in production mode."""
        return self.environment == "production"
    
    @property
    def is_testing(self) -> bool:
        """Check if running in testing mode."""
        return self.environment == "testing"


@lru_cache
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()
