"""
Pytest Configuration
====================

Fixtures and configuration for tests.
"""

import pytest
from fastapi.testclient import TestClient

from parhelion_py.main import app


@pytest.fixture
def client() -> TestClient:
    """Create test client for FastAPI app."""
    return TestClient(app)


@pytest.fixture
def api_prefix() -> str:
    """API prefix for Python endpoints."""
    return "/api/py"
