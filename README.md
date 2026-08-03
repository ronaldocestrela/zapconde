# SmartCondo

Backend SaaS multi-tenant para gestão condominial inteligente em .NET 10.

## Status atual

- Fase em execução: **Fase 1 — Fundação da Infraestrutura**
- Subfase concluída: **1.2.2 — ITenantScoped + Global Query Filter por tenant**
- Próxima subfase: **1.2.3 — Pgvector para RAG**

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
