# Changelog

Historial de cambios del proyecto Parhelion Logistics.

---

## [Unreleased] - En desarrollo

### Pendiente

- Implementar entidades del Domain (Tenant, User, Shipment, etc.)
- Configurar Entity Framework Core y migraciones
- Crear endpoints de la API
- PWA Service Workers para modo offline

---

## [0.3.0] - 2025-12-12

### Agregado

- **Sistema de Diseño Neo-Brutalism**: Estilo visual moderno con bordes sólidos y sombras
  - Paleta "Industrial Solar": Oxide (#C85A17), Sand (#E8E6E1), Black (#000000)
  - Tipografía: New Rocker (logo), Merriweather (títulos), Inter (body)
  - Componentes: Buttons, Cards, Inputs con estilo brutalist
- **Grid Animado**: Fondo con grid cuadriculado naranja y movimiento aleatorio
  - Dirección random en cada carga de página
  - 8 direcciones posibles (cardinales + diagonales)
- **Remote Development**: Frontends configurados para acceso via Tailscale
  - Vite servers escuchando en `0.0.0.0`
  - Backend API accesible remotamente

### Configurado

- **Puertos dedicados** via `.env`:
  - Backend: 5100
  - Admin: 4100
  - Operaciones: 5101
  - Campo: 5102
- **Endpoint `/health`** en backend API para verificación de estado

---

## [0.2.0] - 2025-12-11

### Agregado

- **Docker**: Configuración completa de docker-compose con 6 servicios
  - PostgreSQL 16 con healthcheck
  - Backend API (.NET 8)
  - Frontend Admin (Angular 18)
  - Frontend Operaciones (React + Vite)
  - Frontend Campo (React + Vite)
  - Cloudflare Tunnel para exposición pública
- **Healthchecks**: Todos los servicios tienen verificación de salud
- **CI/CD**: Pipeline de GitHub Actions para build y test automático
- **Red Docker**: Todos los servicios en `parhelion-network`

### Configurado

- Variables de entorno via `.env` (no versionado)
- Cloudflared espera a que todos los servicios estén healthy

---

## [0.1.0] - 2025-12-11

### Agregado

- **Estructura del proyecto**: 4 carpetas principales
  - `backend/`: .NET 8 Web API con Clean Architecture
  - `frontend-admin/`: Angular 18 con routing
  - `frontend-operaciones/`: React + Vite + TypeScript
  - `frontend-campo/`: React + Vite + TypeScript
- **Documentación**:
  - `database-schema.md`: Esquema completo de BD
  - `requirments.md`: Requerimientos funcionales
  - `BRANCHING.md`: Estrategia de ramas Git Flow
- **Git Flow**: Ramas `main` y `develop` configuradas

### Notas

- Las 4 feature branches vacías fueron eliminadas
- Solo se crean branches cuando hay trabajo real

---

## Próximos Pasos

1. Implementar Domain Layer (entidades)
2. Configurar Infrastructure Layer (EF Core)
3. Crear API endpoints básicos
4. Diseñar UI del Admin
5. Probar Docker en local
