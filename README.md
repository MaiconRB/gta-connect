# GTA Connect

Rede social para jogadores de GTA Online (PS5/PS4, GTA V hoje — arquitetura pronta para GTA VI no futuro). Ver [PROJETO.md](PROJETO.md) para a visão completa do produto.

## Stack

- **Backend**: .NET 10, Clean Architecture (`Domain` / `Application` / `Infrastructure` / `Api`), ASP.NET Core Identity + JWT, EF Core + SQL Server.
- **Frontend**: Angular (standalone, signals), Tailwind CSS v4.
- **Banco**: SQL Server 2022 via Docker.

## Pré-requisitos

- .NET SDK 10+
- Node.js 24+ / npm
- Docker Desktop
- Ferramenta `dotnet-ef` (`dotnet tool install --global dotnet-ef`)

## Subindo o ambiente

### 1. Banco de dados

```bash
cp .env.example .env   # ajuste a senha se quiser
docker compose up -d
```

### 2. Backend

```bash
cd backend/src/GtaConnect.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=GtaConnect;User Id=sa;Password=<mesma senha do .env>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:SecretKey" "<uma chave aleatória de 32+ caracteres>"
```

Aplicar as migrations (primeira vez ou após novas migrations):

```bash
cd backend
dotnet ef database update --project src/GtaConnect.Infrastructure --startup-project src/GtaConnect.Api
```

Rodar a API:

```bash
cd backend/src/GtaConnect.Api
dotnet run --urls "http://localhost:5080"
```

Swagger UI: http://localhost:5080/swagger — Health check: http://localhost:5080/health

### 3. Frontend

```bash
cd frontend
npm install
npx ng serve
```

App em http://localhost:4200 (chamadas a `/api/*` são redirecionadas para a API via `proxy.conf.json`).

## Testes

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && npx ng test --watch=false
```
