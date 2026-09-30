# LocalStack.NetApi
A .NET 10 Web API that uses LocalStack to simulate AWS S3 for file storage in local development.

## Architecture 🏗️
![Architecture](api-diagram.png)

![S3 flow with LocalStack](api-diagram-localstack.png)

## Requirements ✅
- 🧰 .NET 10 SDK
- 🐳 Docker (for LocalStack)

## Structure 📦
- **LocalStack.Api** - controllers, configuration, middleware, health checks
- **LocalStack.Services** - storage and item logic
- **LocalStack.Repository** - in-memory item data
- **LocalStack.Models** - DTOs
- **localstack-init** - creates `local-bucket` when the LocalStack container starts

## Run ▶️
From the repo root:

```bash
docker compose up -d localstack
dotnet run --project LocalStack.Api
```

The API listens on `http://localhost:5142`. Swagger (Development): `http://localhost:5142/swagger`.

- `GET /health` - all health checks (the S3 bucket)
- `GET /health/ready` - checks tagged `ready` (S3 / LocalStack)

## LocalStack (S3) 🪣
LocalStack listens on `http://localhost:4566`. The `local-bucket` bucket is created by `localstack-init/init-bucket.sh` when the container starts, and again when the API boots.

Settings are the `LocalStack` section in `LocalStack.Api/appsettings.json` (the same values are in `LocalStack.Api/appsettings.Development.json`). Leave `ServiceUrl` empty to use the default AWS S3 client.

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/v1/file/upload` | Upload a file (`file`, optional `key`). Max 10 MB |
| GET | `/api/v1/file/download/{key}` | Download by key |
| GET | `/api/v1/file/list` | List keys in the bucket |
| HEAD | `/api/v1/file/{key}` | Check if the object exists |
| GET | `/api/v1/file/metadata/{key}` | Size, content type, and ETag |
| GET | `/api/v1/file/presign/{key}` | Presigned URL (`verb`, `expiresInMinutes`) |
| DELETE | `/api/v1/file/{key}` | Delete by key |

Sample requests are in `LocalStackApi.http`.

## Docker 🐳
The image listens on port 80 (`ASPNETCORE_HTTP_PORTS`). This maps it to `http://localhost:8787` and points S3 at LocalStack on the host:

```bash
docker build -f Dockerfile -t localstack-netapi:latest .
docker run -d -p 8787:80 -e ASPNETCORE_ENVIRONMENT=Development -e LocalStack__ServiceUrl=http://host.docker.internal:4566 --name localstack-netapi localstack-netapi:latest
```

Swagger: `http://localhost:8787/swagger`.
