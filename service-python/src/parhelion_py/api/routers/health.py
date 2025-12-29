"""
API Routers - Health Endpoints
==============================

Health check endpoints for service monitoring.
"""

from datetime import datetime, timezone
from typing import Any

from fastapi import APIRouter

from parhelion_py.infrastructure.config.settings import get_settings
from parhelion_py.infrastructure.database.connection import check_db_connection

router = APIRouter()
settings = get_settings()


@router.get("/health")
async def health_check() -> dict[str, Any]:
    """
    Basic health check endpoint.
    
    Returns service status and metadata.
    """
    return {
        "status": "healthy",
        "service": settings.service_name,
        "version": settings.version,
        "environment": settings.environment,
        "timestamp": datetime.now(timezone.utc).isoformat(),
    }


@router.get("/health/db")
async def health_db() -> dict[str, Any]:
    """
    Database connectivity health check.
    
    Verifies connection to PostgreSQL.
    """
    db_healthy = await check_db_connection()
    
    return {
        "status": "healthy" if db_healthy else "unhealthy",
        "database": {
            "connected": db_healthy,
            "type": "postgresql",
        },
        "timestamp": datetime.now(timezone.utc).isoformat(),
    }


@router.get("/health/ready")
async def readiness_check() -> dict[str, Any]:
    """
    Readiness probe for Kubernetes/Docker.
    
    Checks if service is ready to accept traffic.
    """
    db_ready = await check_db_connection()
    
    # In alpha, we're ready even without DB (for testing)
    is_ready = True  # Will be: db_ready when DB is required
    
    return {
        "ready": is_ready,
        "checks": {
            "database": db_ready,
            "config": bool(settings.jwt_secret),
        },
        "timestamp": datetime.now(timezone.utc).isoformat(),
    }
