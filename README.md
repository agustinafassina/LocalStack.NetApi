## TemplateApi (.NET 10) ✨
Starter Web API template using .NET 10 with example controllers, services, mapping and middleware. It provides a foundation for building Web APIs with JWT authentication, global exception handling, and Swagger.<br>
Status: template ready for customization. <br>
It´s a copy of https://github.com/agustinafassina/TemplateApi.Net8

## Requirements ✅
- 🧰 .NET 10 SDK
- 🐳 (Optional) Docker — necesario para LocalStack (S3)

## Solution structure 📦 (MVC + OOP)
- **Template.Api**: entry point, controllers, configs (Swagger, AutoMapper), middleware, validators, request contracts.
- **Template.Services**: application services and interfaces; registered via `AddApplicationServices()`.
- **Template.Repository**: data access (repositories); registered via `AddRepositories()`.
- **Template.Models**: DTOs and shared models.

## Dependency Injection (DI) 🔌
Repositories and services are registered with extension methods so `Program.cs` stays clean:
- `builder.Services.AddRepositories();` — registers all repository interfaces/implementations.
- `builder.Services.AddApplicationServices();` — registers all application services (depends on repositories).

Lifetimes: repositories use **Singleton** for in-memory store; when you switch to Entity Framework, use **Scoped** for `DbContext` and repository implementations.

## Run locally ▶️
1. Restore and build:
```
dotnet build
```

2. Run the API:
```
dotnet run --project Template.Api
```

By default, when running in Development, Swagger should be available at `http://localhost:{port}/swagger`.

## LocalStack (S3) 🪣

La API puede usar **LocalStack** para simular AWS S3 en local sin coste ni credenciales reales.

### 1. Levantar LocalStack

Desde la raíz del repo:

```bash
docker-compose up -d localstack
```

El bucket `local-bucket` se crea automáticamente al iniciar (script en `localstack-init/`). La API también crea el bucket al arrancar si no existe.

### 2. Configuración

En `appsettings.Development.json` (o variables de entorno) se usa por defecto:

- **ServiceUrl**: `http://localhost:4566`
- **BucketName**: `local-bucket`
- **Region**: `us-east-1`
- **AccessKey** / **SecretKey**: `test` (válidos solo para LocalStack)

Para usar AWS real, deja vacío `LocalStack:ServiceUrl` y configura la sección estándar `AWS` (o credenciales del sistema).

### 3. Endpoints de archivos (S3)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/v1/file/upload` | Subir archivo (form-data: `file`; opcional: `key`) |
| `GET`  | `/api/v1/file/download/{key}` | Descargar por clave |
| `GET`  | `/api/v1/file/list` | Listar claves del bucket |
| `DELETE` | `/api/v1/file/{key}` | Borrar objeto por clave |

Ejemplo con curl (con LocalStack y la API en marcha):

```bash
# Subir
curl -X POST "http://localhost:5000/api/v1/file/upload" -F "file=@mi-archivo.txt"

# Listar
curl "http://localhost:5000/api/v1/file/list"

# Descargar (usa la key devuelta al subir)
curl -o descarga.txt "http://localhost:5000/api/v1/file/download/{key}"
```

## Docker 🐳
Build the image and run the container:
```
docker build -f Dockerfile -t templateapi:latest .
docker run -d -p 8787:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name templateapi templateapi:latest
# Swagger: http://localhost:8787/swagger/index.html
```

## Authentication (JWT / Auth0 example) 🔐
The template includes support for JWT authentication. Configure the values in `appsettings.json` or via environment variables. Example JSON configuration:
```
"Auth0App1": {
  "Issuer": "https://your-domain.auth0.com/",
  "Audience": "Your-Audience"
},
"Auth0App2": {
  "Issuer": "https://another-issuer/",
  "Audience": "Another-Audience"
}
```

And in controllers you can use:
```
[Authorize(AuthenticationSchemes = "Auth0App1")]
[Authorize(AuthenticationSchemes = "Auth0App2")]
```

## Request validation ✅
The API uses **FluentValidation** for request DTOs (e.g. `ItemCreateDto`). Validators live in `Template.Api/Validators/` and are registered in DI; controllers inject `IValidator<T>` and validate before calling services.

## Configuration ⚙️
- Use `appsettings.json` and `appsettings.Development.json` for environment-specific values.
- Environment variables prefixed with `ASPNETCORE_` affect host behavior.

## Example: Version endpoint ⚡

This template exposes a simple version endpoint in `ItemController`.

- Request:

  ```
  GET /api/v1/item/version
  ```

- curl example:

  ```bash
  curl -i http://localhost:5000/api/v1/item/version
  ```

- Response 200 (example):

  ```json
  "v.1.0.0"
  ```

## Contributing 🤝
1. Fork the repo
2. Create a branch (`feature/name`)
3. Open a Pull Request describing your changes

## License 📜
This repository is a template created by Agustina Fassina.