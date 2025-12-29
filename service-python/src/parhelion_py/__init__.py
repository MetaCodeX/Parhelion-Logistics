"""
Parhelion Python Analytics Service
===================================

Microservicio para análisis avanzado y predicciones del sistema Parhelion Logistics.

Estructura Clean Architecture:
- domain/: Entidades, Value Objects, Interfaces (sin dependencias externas)
- application/: DTOs, Services, Use Cases
- infrastructure/: Database, External Clients, Config
- api/: FastAPI Routers, Middleware
"""

__version__ = "0.6.0-alpha"
__author__ = "MetaCodeX"
