# SmartCondo

Backend SaaS multi-tenant para gestão condominial inteligente em .NET 10.

## Status atual

- Fase em execução: **Fase 1 — Fundação da Infraestrutura**
- Subfase concluída: **1.1.1 — Estrutura Inicial do Modular Monolith**
- Próxima subfase: **1.1.2 — Configuração do SmartCondo.Api (Minimal APIs + FastEndpoints)**

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

## Referências

- Instruções-mestre: `AGENTS.md`
- Especificação funcional: `FUNCTIONAL_SPEC.md`
- Plano de execução: `ROADMAP.md`
