# ConexaBackend

Backend ASP.NET Core 8 para gestión de películas Star Wars. Integra la API pública [SWAPI](https://www.swapi.tech/), autenticación JWT con roles y CRUD de películas.

## Características

- Registro e inicio de sesión con JWT
- Roles: `User` (regular) y `Admin`
- Listado paginado de películas
- Detalle de película (usuarios autenticados con rol User o Admin)
- CRUD de películas (solo Admin)
- Sincronización del catálogo desde SWAPI (solo Admin)
- Documentación Swagger
- Tests unitarios e integración

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Estructura del proyecto

```
src/
├── Conexa.Api/              # Web API, controllers, Swagger
├── Conexa.Application/      # Servicios, DTOs, validadores
├── Conexa.Domain/             # Entidades y enums
└── Conexa.Infrastructure/     # EF Core, Identity, JWT, SWAPI client
tests/
└── Conexa.Tests/              # xUnit + Moq + WebApplicationFactory
```

## Ejecución local

```bash
# Restaurar dependencias y compilar
dotnet restore
dotnet build

# Aplicar migraciones (automático al iniciar, también manual):
dotnet ef database update --project src/Conexa.Infrastructure --startup-project src/Conexa.Api

# Ejecutar la API
dotnet run --project src/Conexa.Api
```

La API estará disponible en:

- HTTP: `http://localhost:5201`
- Swagger: `http://localhost:5201/swagger`
- Health check: `http://localhost:5201/health`

## Credenciales seed (Development)

Al iniciar por primera vez se crean automáticamente:

| Campo    | Valor              |
|----------|--------------------|
| Email    | `admin@conexa.com` |
| Password | `Admin123!`        |
| Rol      | `Admin`            |

Si la base está vacía, también se sincronizan las películas desde SWAPI al arrancar.

## Configuración

Editar `src/Conexa.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=conexa.db"
  },
  "Jwt": {
    "Issuer": "ConexaBackend",
    "Audience": "ConexaBackend",
    "SecretKey": "CHANGE-ME-IN-PRODUCTION-MIN-32-CHARS!!",
    "ExpirationMinutes": 60
  },
  "Swapi": {
    "BaseUrl": "https://www.swapi.tech/api/"
  }
}
```

### PostgreSQL (producción)

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=conexa;Username=postgres;Password=secret"
dotnet run --project src/Conexa.Api
```

Si la connection string contiene `Host=`, se usa PostgreSQL; de lo contrario, SQLite.

## Endpoints principales

| Método | Ruta | Auth | Rol |
|--------|------|------|-----|
| POST | `/api/auth/register` | No | — |
| POST | `/api/auth/login` | No | — |
| GET | `/api/movies` | JWT | Cualquiera |
| GET | `/api/movies/{id}` | JWT | User, Admin |
| POST | `/api/movies` | JWT | Admin |
| PUT | `/api/movies/{id}` | JWT | Admin |
| DELETE | `/api/movies/{id}` | JWT | Admin |
| POST | `/api/movies/sync` | JWT | Admin |

## Flujo de prueba sugerido

1. Abrir Swagger en `/swagger`
2. `POST /api/auth/login` con credenciales admin → copiar el `token`
3. Clic en **Authorize** → ingresar `Bearer {token}`
4. `POST /api/movies/sync` → importar películas SWAPI
5. `GET /api/movies` → ver listado paginado
6. `GET /api/movies/{id}` → ver detalle
7. Registrar un usuario regular con `POST /api/auth/register`
8. Con token de User: acceder a detalle (OK), crear película (403 Forbidden)
9. Con token de Admin: CRUD completo

## Tests

```bash
dotnet test
```

Incluye tests de:

- Autenticación (register, login, conflictos)
- Lógica de películas (CRUD, paginación)
- Sincronización SWAPI (upsert create/update)
- Autorización de endpoints (401, 403, 201)
- Generación de JWT con roles

## Documentación API

Swagger UI disponible en `/swagger` cuando la aplicación corre en entorno Development.
