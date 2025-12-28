"""
Parhelion Python Analytics Service - Main Application
======================================================

FastAPI application entry point with Clean Architecture.
"""

from contextlib import asynccontextmanager
from typing import AsyncGenerator

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from parhelion_py.api.routers import health
from parhelion_py.infrastructure.config.settings import get_settings

settings = get_settings()


@asynccontextmanager
async def lifespan(app: FastAPI) -> AsyncGenerator[None, None]:
    """Application lifespan handler for startup/shutdown events."""
    # Startup
    print(f"🚀 Starting Parhelion Python Analytics v{settings.version}")
    print(f"📊 Environment: {settings.environment}")
    print(f"🔗 Database: {settings.database_url.split('@')[-1] if settings.database_url else 'Not configured'}")
    
    yield
    
    # Shutdown
    print("👋 Shutting down Parhelion Python Analytics")


def create_app() -> FastAPI:
    """Factory function to create FastAPI application."""
    
    app = FastAPI(
        title="Parhelion Python Analytics",
        description="Microservicio de análisis y predicciones para Parhelion Logistics",
        version=settings.version,
        docs_url="/docs" if settings.environment != "production" else None,
        redoc_url="/redoc" if settings.environment != "production" else None,
        lifespan=lifespan,
    )
    
    # CORS Middleware
    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.cors_origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )
    
    # Include routers
    app.include_router(health.router, tags=["Health"])
    
    return app


# Application instance
app = create_app()


if __name__ == "__main__":
    import uvicorn
    
    uvicorn.run(
        "parhelion_py.main:app",
        host="0.0.0.0",
        port=8000,
        reload=settings.environment == "development",
    )
