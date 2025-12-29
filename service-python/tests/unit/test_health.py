"""
Health Endpoint Tests
=====================

Tests for /health, /health/db, /health/ready endpoints.
"""

from fastapi.testclient import TestClient


class TestHealthEndpoints:
    """Test suite for health check endpoints."""

    def test_health_returns_200(self, client: TestClient) -> None:
        """Test basic health endpoint returns 200."""
        response = client.get("/health")
        
        assert response.status_code == 200
        data = response.json()
        assert data["status"] == "healthy"
        assert "version" in data
        assert "timestamp" in data

    def test_health_contains_service_info(self, client: TestClient) -> None:
        """Test health endpoint contains service metadata."""
        response = client.get("/health")
        data = response.json()
        
        assert data["service"] == "python-analytics"
        assert data["version"] == "0.6.0-alpha"
        assert data["environment"] in ["development", "production", "testing"]

    def test_health_db_returns_200(self, client: TestClient) -> None:
        """Test database health endpoint returns 200."""
        response = client.get("/health/db")
        
        assert response.status_code == 200
        data = response.json()
        assert "status" in data
        assert "database" in data
        assert data["database"]["type"] == "postgresql"

    def test_health_ready_returns_200(self, client: TestClient) -> None:
        """Test readiness probe returns 200."""
        response = client.get("/health/ready")
        
        assert response.status_code == 200
        data = response.json()
        assert "ready" in data
        assert "checks" in data
        assert "timestamp" in data
