"""
Infrastructure Database - Connection
=====================================

Async SQLAlchemy engine and session management.
"""

from collections.abc import AsyncGenerator
from typing import Annotated

from fastapi import Depends
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession, async_sessionmaker, create_async_engine

from parhelion_py.infrastructure.config.settings import get_settings

settings = get_settings()

# Create async engine (lazy initialization)
_engine = None
_async_session_factory = None


def get_engine():
    """Get or create async engine."""
    global _engine
    if _engine is None and settings.database_url:
        _engine = create_async_engine(
            settings.database_url,
            echo=not settings.is_production,
            pool_size=5,
            max_overflow=10,
            pool_pre_ping=True,
        )
    return _engine


def get_session_factory():
    """Get or create session factory."""
    global _async_session_factory
    engine = get_engine()
    if _async_session_factory is None and engine is not None:
        _async_session_factory = async_sessionmaker(
            engine,
            class_=AsyncSession,
            expire_on_commit=False,
            autoflush=False,
        )
    return _async_session_factory


async def get_db_session() -> AsyncGenerator[AsyncSession, None]:
    """Dependency to get database session."""
    factory = get_session_factory()
    if factory is None:
        raise RuntimeError("Database not configured. Set DATABASE_URL environment variable.")
    
    async with factory() as session:
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise


# Type alias for dependency injection
DbSession = Annotated[AsyncSession, Depends(get_db_session)]


async def check_db_connection() -> bool:
    """Check if database connection is working."""
    engine = get_engine()
    if engine is None:
        return False
    
    try:
        async with engine.connect() as conn:
            await conn.execute(text("SELECT 1"))
        return True
    except Exception:
        return False
