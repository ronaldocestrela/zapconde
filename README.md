# SmartCondo

Backend SaaS multi-tenant para gestão condominial inteligente em .NET 10.

## Status atual

- Fase em execução: **Fase 1 — Fundação da Infraestrutura**
- Subfase concluída: **1.2.2 — ITenantScoped + Global Query Filter por tenant**
- Próxima subfase: **1.2.3 — Pgvector para RAG**

## Execução em desenvolvimento (teste local)

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-docker/) + Docker Compose (Docker Desktop com integração WSL, se aplicável)
- Portas livres: `5432` (Postgres), `6379` (Redis), `5672` / `15672` (RabbitMQ), `5127` (API), `5181` (Web)

### 1. Subir a infraestrutura

Na raiz do repositório:

```bash
# Opcional: sobrescrever variáveis (credenciais padrão já batem com Development)
cp .env.example .env

docker compose up -d
docker compose ps
```

Serviços e endpoints:

| Serviço   | Host / porta              | Credenciais / notas                          |
|-----------|---------------------------|----------------------------------------------|
| Postgres  | `localhost:5432`          | `postgres` / `postgres` · DB `smartcondo_dev` (+ pgvector) |
| Redis     | `localhost:6379`          | sem senha                                    |
| RabbitMQ  | `localhost:5672`          | `guest` / `guest`                            |
| RabbitMQ UI | http://localhost:15672  | `guest` / `guest`                            |

As connection strings em `src/API/SmartCondo.Api/appsettings.Development.json` já apontam para esses valores.

Para parar:

```bash
docker compose down        # mantém volumes
docker compose down -v     # remove volumes (apaga dados)
```

### 2. Restaurar e compilar

```bash
dotnet restore SmartCondo.slnx
dotnet build SmartCondo.slnx
```

### 3. Subir a API

```bash
dotnet run --project src/API/SmartCondo.Api --launch-profile http
```

URLs úteis:

- API: http://localhost:5127
- Health (liveness): http://localhost:5127/health/live
- Health (readiness): http://localhost:5127/health/ready
- OpenAPI / Scalar: http://localhost:5127/scalar
- Health resumido: http://localhost:5127/api/health

Com `Database:MigrateOnStartup` e `Identity:SeedOnStartup` ativos no Development, as migrations e o seed de identidade rodam na inicialização (nesta ordem).

> **Nota sobre logs no primeiro arranque:** ao aplicar migrations em um banco vazio, o EF Core pode registrar um `fail: Microsoft.EntityFrameworkCore.Database.Command[20102]` no `SELECT` de `__EFMigrationsHistory` (a tabela ainda não existe). Isso é esperado. Em seguida devem aparecer a criação da tabela de histórico, a aplicação das migrations e `Application started`.

Se a API **não subir**, verifique:

1. `docker compose ps` — Postgres healthy
2. Banco `smartcondo_dev` existe (criado pelo compose)
3. Connection string em `appsettings.Development.json` aponta para `smartcondo_dev`
4. A **exception completa** abaixo do log `20102` (ex.: `database does not exist`, Postgres offline)

### 4. Subir o frontend Blazor (opcional)

Em outro terminal:

```bash
dotnet run --project src/Web/SmartCondo.Web --launch-profile http
```

- Web: http://localhost:5181
- `ApiBaseUrl` em `src/Web/SmartCondo.Web/appsettings.Development.json` deve apontar para a API (`http://localhost:5127` se usar o perfil `http` da API).

### 5. Rodar os testes

Testes de arquitetura e unidade (sem Docker da aplicação; integração usa Testcontainers):

```bash
dotnet test tests/Tests.Architecture
dotnet test tests/Tests.Unit
dotnet test tests/Tests.Integration
```

Ou a solução inteira:

```bash
dotnet test SmartCondo.slnx
```

> Os testes de integração sobem Postgres/Redis/RabbitMQ via **Testcontainers** e precisam do Docker em execução. O `docker compose` da raiz é para a API/Web em modo Development, não substitui os containers dos testes.

### Checklist rápido

1. `docker compose up -d` e aguardar health dos serviços
2. `dotnet run --project src/API/SmartCondo.Api --launch-profile http`
3. Validar http://localhost:5127/health/ready
4. (Opcional) subir o Web e/ou `dotnet test`

## Estrutura inicial implementada

- `src/BuildingBlocks`
  - `BuildingBlocks.Domain`
  - `BuildingBlocks.Infrastructure`
  - `BuildingBlocks.Shared`
- `src/Modules`
  - `Identity`, `Financial`, `Operations`, `AccessControl`, `WhatsApp`, `AIEngine`
- `src/API`
  - `SmartCondo.Api`
- `tests`
  - `Tests.Architecture`, `Tests.Unit`, `Tests.Integration`

## Qualidade e baseline

- Solução `SmartCondo.slnx` com todos os projetos registrados.
- Padronização com `Directory.Build.props` e `.editorconfig`.
- Testes de conformidade estrutural em `tests/Tests.Architecture/StructuralConformityTests.cs`.
- Testes de persistência da Subfase 1.2.1 em:
  - `tests/Tests.Architecture/InfrastructurePersistenceConfigurationTests.cs`
  - `tests/Tests.Integration/Infrastructure/PostgreSqlConnectivityTests.cs`
- **Testes de multi-tenancy da Subfase 1.2.2 em:**
  - `tests/Tests.Architecture/MultiTenancyArchitectureTests.cs` (7 testes aprovados)
  - `tests/Tests.Unit/Infrastructure/MultiTenantDbContextTests.cs` (5 testes aprovados)
  - `tests/Tests.Integration/Infrastructure/MultiTenantDbContextIntegrationTests.cs` (4 testes aprovados)
  - Documentação viva: `tests/LivingDoc/Features/Fase1_2_2_TenantGlobalQueryFilter.feature`

## Referências

- Instruções-mestre: `AGENTS.md`
- Especificação funcional: `FUNCTIONAL_SPEC.md`
- Plano de execução: `ROADMAP.md`
