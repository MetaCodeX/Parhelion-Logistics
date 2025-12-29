# Parhelion Python Analytics Service

**Version:** 0.6.0-alpha  
**Framework:** FastAPI + Python 3.12  
**Architecture:** Clean Architecture

## Estructura

```
src/parhelion_py/
├── main.py                    # FastAPI entry point
├── domain/                    # 🔷 Core business logic (no dependencies)
│   ├── entities/              # Domain entities
│   ├── value_objects/         # Immutable value objects
│   ├── exceptions/            # Domain exceptions
│   └── interfaces/            # Repository interfaces (Ports)
├── application/               # 🔶 Use cases and DTOs
│   ├── dtos/                  # Data Transfer Objects
│   ├── services/              # Application services
│   └── interfaces/            # External service interfaces
├── infrastructure/            # 🔵 External implementations
│   ├── config/                # Settings (Pydantic)
│   ├── database/              # SQLAlchemy async
│   └── external/              # HTTP clients (.NET API)
└── api/                       # 🟢 HTTP layer
    ├── routers/               # FastAPI routers
    └── middleware/            # Auth, tenant, logging
```

## Desarrollo Local

```bash
# Crear virtual environment
python -m venv .venv
source .venv/bin/activate  # Linux/Mac
# .venv\Scripts\activate   # Windows

# Instalar dependencias
pip install -e ".[dev]"

# Ejecutar servidor de desarrollo
uvicorn parhelion_py.main:app --reload --port 8000

# Ejecutar tests
pytest tests/ -v

# Linting
ruff check src/
mypy src/
```

## Docker

```bash
# Build
docker build -t parhelion-python .

# Run
docker run -p 8000:8000 \
  -e DATABASE_URL="postgresql+asyncpg://user:pass@host:5432/db" \
  -e JWT_SECRET="your-secret" \
  parhelion-python
```

## Endpoints

| Endpoint        | Método | Descripción             |
| --------------- | ------ | ----------------------- |
| `/health`       | GET    | Estado del servicio     |
| `/health/db`    | GET    | Conectividad PostgreSQL |
| `/health/ready` | GET    | Readiness probe         |
| `/docs`         | GET    | Swagger UI (dev only)   |

## Variables de Entorno

| Variable               | Requerida | Default                     | Descripción                    |
| ---------------------- | --------- | --------------------------- | ------------------------------ |
| `DATABASE_URL`         | No        | -                           | PostgreSQL async connection    |
| `JWT_SECRET`           | Sí        | -                           | Secret para validar tokens     |
| `INTERNAL_SERVICE_KEY` | Sí        | -                           | Key para auth inter-servicios  |
| `PARHELION_API_URL`    | No        | `http://parhelion-api:5000` | URL del API .NET               |
| `ENVIRONMENT`          | No        | `development`               | development/production/testing |

---

**Bounded Context:** Analytics & Predictions  
**Puerto:** 8000  
**Container:** parhelion-python
