# 🚀 `ROADMAP.md` — Ordem Lógica de Execução e Desenvolvimento

Este plano de execução foi estruturado em **Fases Funcionais e Arquiteturais encadeadas**. Cada subfase deve ser implementada obrigatoriamente através do ciclo **TDD (Red $\rightarrow$ Green $\rightarrow$ Refactor)**, validada por testes e documentada em BDD com o arquivo `.feature` correspondente.

---

## 🏗️ Fase 1: Fundação da Infraestrutura e Núcleo Multi-tenant (.NET 10)

> 
> **Objetivo:** Estabelecer o ecossistema base do projeto, o isolamento seguro de dados por `tenant_id` e a infraestrutura de dados e mensageria.
> 
> 

### 1.1. Setup da Solução e Injeção de Dependências

* [x] **Subfase 1.1.1:** Criar a estrutura de pastas do **Modular Monolith** (`src/BuildingBlocks`, `src/Modules`, `src/API`).

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Estrutura base criada em `src/BuildingBlocks`, `src/Modules`, `src/API` e `tests`.
  - Projetos .NET 10 materializados para BuildingBlocks, módulos, API e testes.
  - Solução `SmartCondo.slnx` criada e com todos os projetos registrados.
  - Baseline técnico criado com `Directory.Build.props`, `.editorconfig` e `.gitignore`.
  - Testes de conformidade arquitetural implementados em `tests/Tests.Architecture/StructuralConformityTests.cs`.
  - Build e testes da fase executados com sucesso.


* [x] **Subfase 1.1.2:** Configurar o projeto `SmartCondo.Api` em .NET 10 com **Minimal APIs** / **FastEndpoints**.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Contrato `Result` e `Result<T>` criado em `BuildingBlocks.Shared` conforme `AGENTS.md`.
  - FastEndpoints 5.40.0 configurado no projeto API.
  - Endpoint `/api/health` implementado retornando `Result<HealthDto>`.
  - `Program.cs` refatorado e organizado com métodos de extensão (`ServiceCollectionExtensions`, `ApplicationBuilderExtensions`).
  - Template `WeatherForecast` removido completamente.
  - Arquivo BDD criado: `tests/LivingDoc/Features/Fase1_1_2_ApiBootstrap.feature`.
  - Testes de integração criados em `tests/Tests.Integration/Api/ApiBootstrapTests.cs` (6 casos de teste).
  - Testes arquiteturais criados em `tests/Tests.Architecture/ApiConfigurationTests.cs` (4 casos de teste).
  - **Todos os 24 testes da solução passaram** (TDD Red → Green completo).
  - API inicializa em ~114ms com 1 endpoint registrado.


* [x] **Subfase 1.1.3:** Adicionar suporte a documentação OpenAPI/Scalar com geração automática a partir dos XML Comments.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - `SmartCondo.Api` configurada para gerar arquivo XML de documentação no build (`GenerateDocumentationFile=true`).
  - Documento OpenAPI v1 exposto em `/openapi/v1.json` no ambiente de desenvolvimento.
  - UI de documentação Scalar habilitada em `/scalar` no ambiente de desenvolvimento.
  - Comentários XML públicos enriquecendo o contrato OpenAPI (ex.: `HealthDto`).
  - Arquivo BDD criado: `tests/LivingDoc/Features/Fase1_1_3_ApiDocumentation.feature`.
  - Testes de integração estendidos em `tests/Tests.Integration/Api/ApiBootstrapTests.cs` para OpenAPI/Scalar/XML docs.
  - Testes arquiteturais estendidos em `tests/Tests.Architecture/ApiConfigurationTests.cs` para Scalar e geração de XML.
  - Build da solução e testes relevantes da subfase executados com sucesso.



### 1.2. Banco de Dados Relacional e Isolamento Multi-tenant

* [x] **Subfase 1.2.1:** Configurar o EF Core 10 com driver PostgreSQL (`Npgsql`) no `BuildingBlocks.Infrastructure`.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - `BuildingBlocks.Infrastructure` atualizado com:
    - `Microsoft.EntityFrameworkCore` `10.0.10`
    - `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.0`
  - Configuração de `ConnectionStrings:Postgres` em:
    - `src/API/SmartCondo.Api/appsettings.json`
    - `src/API/SmartCondo.Api/appsettings.Development.json` (override por ambiente)
  - Composição de infraestrutura adicionada via:
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
    - `src/API/SmartCondo.Api/Configuration/ServiceCollectionExtensions.cs`
    - `src/API/SmartCondo.Api/Program.cs`
  - Referência de projeto adicionada:
    - `src/API/SmartCondo.Api/SmartCondo.Api.csproj` -> `BuildingBlocks.Infrastructure`
  - BDD da subfase criado:
    - `tests/LivingDoc/Features/Fase1_2_1_EfCoreNpgsqlSetup.feature`
  - Testes criados:
    - `tests/Tests.Architecture/InfrastructurePersistenceConfigurationTests.cs`
    - `tests/Tests.Integration/Infrastructure/PostgreSqlConnectivityTests.cs`
  - Build e testes relevantes da subfase executados com sucesso.


* [x] **Subfase 1.2.2:** Criar a interface `ITenantScoped` e implementar o **Global Query Filter** no EF Core para forçar `e.TenantId == CurrentTenantId` em todas as consultas.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Contratos de multi-tenancy criados em `BuildingBlocks.Shared/MultiTenancy`:
    - `ITenantScoped` (interface com `TenantId` para entidades)
    - `ICurrentTenantService` (interface de contexto de tenant)
  - Infraestrutura de multi-tenancy criada em `BuildingBlocks.Infrastructure/MultiTenancy`:
    - `CurrentTenantService` (implementação padrão com comportamento deny-by-default)
  - DbContext base reutilizável criado:
    - `BuildingBlocks.Infrastructure/Persistence/MultiTenantDbContext`
    - Aplica `HasQueryFilter` automaticamente em entidades `ITenantScoped` via reflexão
    - Filtro: `entity.TenantId == _currentTenantService.TenantId` (consultas vazias quando tenant não resolvido)
  - DI ajustado em `InfrastructureServiceCollectionExtensions`:
    - `ICurrentTenantService` registrado como Scoped para isolamento por requisição
  - Referência de projeto adicionada:
    - `BuildingBlocks.Infrastructure.csproj` → `BuildingBlocks.Shared`
  - BDD da subfase criado:
    - `tests/LivingDoc/Features/Fase1_2_2_TenantGlobalQueryFilter.feature` (8 cenários)
  - Testes criados e aprovados (TDD Green):
    - `tests/Tests.Architecture/MultiTenancyArchitectureTests.cs` (7 testes - validação de localização e dependências)
    - `tests/Tests.Unit/Infrastructure/MultiTenantDbContextTests.cs` (5 testes - validação de aplicação de filtro)
    - `tests/Tests.Integration/Infrastructure/MultiTenantDbContextIntegrationTests.cs` (4 testes - validação com PostgreSQL real via Testcontainers)
  - Build e todos os 16 testes da subfase executados com sucesso (100% Green).


* [x] **Subfase 1.2.3:** Configurar suporte ao **Pgvector** (`Pgvector.EntityFrameworkCore`) no PostgreSQL para a futura busca vetorial do RAG.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Pacote `Pgvector.EntityFrameworkCore` (versão 0.3.0) adicionado em:
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj`
  - Configuração reutilizável de suporte vetorial criada:
    - `BuildingBlocks.Infrastructure/Persistence/VectorDbContextOptionsExtensions.cs`
    - Métodos de extensão `UseNpgsqlWithVector()` (genérico e não-genérico) que habilitam tipos vetoriais via `NpgsqlDataSourceBuilder`
  - Compatibilidade preservada com arquitetura multi-tenant existente (sem quebra de isolamento)
  - BDD da subfase criado:
    - `tests/LivingDoc/Features/Fase1_2_3_PgvectorSupport.feature` (8 cenários)
  - Testes criados e aprovados (TDD Red → Green):
    - `tests/Tests.Architecture/InfrastructurePersistenceConfigurationTests.cs` (teste adicional para Pgvector.EntityFrameworkCore)
    - `tests/Tests.Integration/Infrastructure/PgvectorSupportTests.cs` (4 testes - validação com PostgreSQL real + pgvector via Testcontainers)
      - Extensão pgvector habilitada no PostgreSQL
      - Mapeamento EF Core de tipo `vector`
      - Persistência e leitura de embeddings vetoriais
      - Consultas de similaridade vetorial com ordenação por distância L2
      - Isolamento multi-tenant preservado em embeddings
  - Build da solução e todos os testes relevantes da subfase executados com sucesso (100% Green).
  - Base técnica preparada para implementação futura do módulo AIEngine/RAG.



### 1.3. Mensageria, Outbox Pattern e Caching

* [x] **Subfase 1.3.1:** Configurar o **MassTransit** com **RabbitMQ** para mensageria assíncrona orientada a eventos.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Dependências de mensageria adicionadas:
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj`
      - `MassTransit` `8.5.2`
      - `MassTransit.RabbitMQ` `8.5.2`
    - `tests/Tests.Integration/Tests.Integration.csproj`
      - `Testcontainers.RabbitMq` `4.8.1`
  - Configuração dedicada de broker adicionada em:
    - `src/API/SmartCondo.Api/appsettings.json`
    - `src/API/SmartCondo.Api/appsettings.Development.json`
    - Seção `RabbitMQ` com `Host`, `Port`, `VirtualHost`, `Username`, `Password`
  - Contrato compartilhado para integração criado:
    - `src/BuildingBlocks/BuildingBlocks.Shared/Messaging/IIntegrationEvent.cs`
  - Bootstrap de mensageria implementado na infraestrutura:
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/Messaging/RabbitMqOptions.cs`
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
      - Validação de configuração RabbitMQ
      - Registro `AddMassTransit` com transporte RabbitMQ
      - Suporte a `ConnectionStrings:RabbitMQ` (URI completa) com fallback para seção dedicada
  - Health check explícito de RabbitMQ implementado:
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/Messaging/RabbitMqBusHealthCheck.cs`
    - Endpoints de saúde mapeados em:
      - `src/API/SmartCondo.Api/Configuration/ApplicationBuilderExtensions.cs`
      - `/health/live` e `/health/ready` (sem conflito com `/api/health`)
  - BDD da subfase criado:
    - `tests/LivingDoc/Features/Fase1_3_1_MassTransitRabbitMqBootstrap.feature`
  - Testes criados e aprovados:
    - `tests/Tests.Architecture/MessagingConfigurationArchitectureTests.cs`
      - valida referências de pacotes, tipo `RabbitMqOptions` e seção `RabbitMQ` nos appsettings
    - `tests/Tests.Integration/Infrastructure/RabbitMqMessagingBootstrapIntegrationTests.cs`
      - valida exposição de readiness e coexistência com endpoint funcional `/api/health`
  - Build da solução e testes relevantes executados com sucesso (Green):
    - Arquitetura: **33/33 aprovados**
    - Integração: **21/21 aprovados**


* [x] **Subfase 1.3.2:** Habilitar o **Transactional Outbox Pattern** no EF Core/MassTransit para garantir idempotência.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Dependências de outbox adicionadas:
    - Pacote `MassTransit.EntityFrameworkCore` `8.5.2` adicionado em `BuildingBlocks.Infrastructure`, `Tests.Architecture`, `Tests.Unit` e `Tests.Integration`.
  - Integração com `MultiTenantDbContext`:
    - `MultiTenantDbContext.OnModelCreating` atualizado com `modelBuilder.AddTransactionalOutboxEntities()`.
    - Entidades do MassTransit Outbox (`OutboxMessage`, `OutboxState`, `InboxState`) isentas automaticamente do Global Query Filter de `ITenantScoped`.
  - Método de extensão de infraestrutura:
    - `AddMassTransitOutbox<TDbContext>()` criado em `InfrastructureServiceCollectionExtensions.cs` para habilitar `UsePostgres()` e `UseBusOutbox()`.
  - Especificação BDD da subfase criada:
    - `tests/LivingDoc/Features/Fase1_3_2_TransactionalOutboxPattern.feature`
  - Suíte de testes (TDD Red → Green):
    - `tests/Tests.Architecture/OutboxConfigurationArchitectureTests.cs` (2 testes de conformidade)
    - `tests/Tests.Unit/Infrastructure/TransactionalOutboxUnitTests.cs` (2 testes unitários de mapeamento e isenção de filtro de tenant)
    - `tests/Tests.Integration/Infrastructure/TransactionalOutboxIntegrationTests.cs` (teste de integração ponta a ponta com PostgreSQL real via Testcontainers validando commit atômico de entidades de domínio e gravação na outbox)
  - Execução e aprovação de 100% dos testes da solução (55/55 aprovados: 35 arquitetura, 8 unidade, 12 integração).


* [x] **Subfase 1.3.3:** Configurar cliente Redis (`StackExchange.Redis`) para controle de cache, Distributed Locks e sessão de chat.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Dependências de Redis e Caching adicionadas:
    - `src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj`: `StackExchange.Redis` `2.8.24`
    - `tests/Tests.Integration/Tests.Integration.csproj`: `Testcontainers.Redis` `4.8.1`
  - Abstrações criadas em `BuildingBlocks.Shared/Caching`:
    - `ICacheService` (operações de cache com isolamento por tenant `tenant:{tenant_id}:{key}`)
    - `IDistributedLockService` e `IDistributedLockHandle` (locks assíncronos descartáveis via `IAsyncDisposable`)
    - `IChatSessionService` (gestão do estado conversacional de chat/WhatsApp com TTL configurável)
  - Implementações de Infraestrutura em `BuildingBlocks.Infrastructure/Caching`:
    - `RedisCacheService` (com serialização JSON via `System.Text.Json` e prefixação automática de tenant)
    - `RedisDistributedLockService` e `RedisDistributedLockHandle` (baseado em `LockTakeAsync` / `LockReleaseAsync`)
    - `RedisChatSessionService` (armazenamento efêmero de sessão com expiração)
    - `RedisHealthCheck` (validação de operatividade e latência do Redis)
  - Configuração e Composição na Injeção de Dependências:
    - Seção `ConnectionStrings:Redis` adicionada nos arquivos `appsettings.json` e `appsettings.Development.json`
    - `InfrastructureServiceCollectionExtensions.cs` atualizado com registro do `IConnectionMultiplexer`, `ICacheService`, `IDistributedLockService`, `IChatSessionService` e `RedisHealthCheck` sob as tags `ready` e `redis`
  - Especificação BDD da subfase criada:
    - `tests/LivingDoc/Features/Fase1_3_3_RedisCacheAndDistributedLock.feature`
  - Suíte de Testes (TDD Red → Green):
    - `tests/Tests.Architecture/RedisConfigurationArchitectureTests.cs` (validação de referências, interfaces e connection string)
    - `tests/Tests.Unit/Infrastructure/RedisCacheAndLockUnitTests.cs` (testes unitários de chaves com tenant e descartabilidade de lock)
    - `tests/Tests.Integration/Infrastructure/RedisIntegrationTests.cs` (testes de integração ponta a ponta com Redis real via Testcontainers)
  - Execução e aprovação de 100% dos testes unitários e de arquitetura da solução (38 testes de arquitetura, 11 testes unitários aprovados).



---

## 🔐 Fase 2: Módulo Identity, Autenticação e Cadastro Base

> 
> **Objetivo:** Garantir a autenticação de usuários, mapear a estrutura física do condomínio e vincular os números de celular ao WhatsApp.
> 
> 

### 2.1. ASP.NET Core Identity e JWT

* [x] **Subfase 2.1.1:** Mapear e aplicar tabelas do Identity customizadas (gerando JWT com claims obrigatórias: `TenantId`, `CondoId`, `UserId`, `Role`).

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Módulo `Modules.Identity` com `ApplicationUser`, `ApplicationRole`, `UserCondoMembership`, `UserRefreshToken` e `SmartCondoClaimTypes`.
  - `IdentityDbContext` + migration `InitialIdentityOpenIddict` (Identity + OpenIddict EF Core).
  - OpenIddict configurado como authorization server; JWT emitido via `IdentityTokenService` com claims `TenantId`, `CondoId`, `UserId`, `Role`.
  - FastEndpoints: `POST /api/auth/login`, `/select-profile`, `/forgot-password`, `/refresh` retornando `Result<T>`.
  - BDD: `tests/LivingDoc/Features/Fase2_1_1_IdentityJwt.feature`.
  - Testes: unitários, arquitetura e integração (`IdentityAuthIntegrationTests`) — Green.
  - App Blazor `src/Web/SmartCondo.Web` com telas Stitch: Login (desktop/mobile), Recuperar Senha, Selecionar Perfil, Acesso Negado + CSS tokens Manrope/`#0f436f`.
  - Asset logo baixado via `curl -L` em `wwwroot/images/auth/zapcond-logo.png`.


* [x] **Subfase 2.1.2:** Criar Middleware de Injeção de Contexto (`CurrentTenantService`) para extrair o `TenantId` do Token JWT ou do Header do Webhook.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - `ICurrentTenantService` estendido com `CondoId`, `SetTenantId`, `SetCondoId` e `Clear()`; constantes `TenantHttpHeaders` (`X-Tenant-Id`, `X-Condo-Id`).
  - `TenantContextMiddleware` + `UseTenantContext()` registrado após `UseAuthentication` em `ApplicationBuilderExtensions`.
  - Resolução: JWT autenticado (claims `TenantId`/`CondoId`) ou header em rotas `/api/webhooks/*`; deny-by-default.
  - Endpoints: `GET /api/auth/context`, `GET /api/auth/profiles`, `GET /api/webhooks/context-probe` retornando `Result<T>`.
  - Seed multi-tenant com `DisplayLabel` (Ville de Paris, Jardim das Flores, Belvedere) + migration `AddMembershipDisplayLabel`.
  - BDD: `tests/LivingDoc/Features/Fase2_1_2_TenantContextMiddleware.feature`.
  - Testes: unitários (`TenantContextMiddlewareTests`), integração (`TenantContextIntegrationTests`), arquitetura (`TenantContextArchitectureTests`) — Green.
  - Blazor: `AppShellLayout`, `TenantSwitcher` (dropdown/recentes/busca), `ContextSwitchModal`, estados single-tenant/empty + `context.css` Stitch.
  - Referências visuais Stitch em `wwwroot/images/context/references/`.



### 2.2. Cadastro de Domínio Condominial

* [x] **Subfase 2.2.1:** Entidade `Condominio` e `Administradora` (Criação de Tenant).

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Entidades `Administradora`, `Condominio`, `Endereco`, `ConfiguracoesIniciais`, enums (`LicensePlan`, `CondominioTipo`, `BankGateway`) e `CnpjValidator` em `Modules.Identity.Domain`.
  - EF Core: configs + migration `AddAdministradoraCondominio`; query filters em `IdentityDbContext`; seed demo alinhado a memberships (tenants 1/2/3).
  - Serviços: `TenantOnboardingService` (transação), `OnboardingDraftService` (Redis), `CnpjLookupService`, `CepLookupService` (ViaCEP/stub Testing).
  - FastEndpoints `Result<T>`: `POST/GET /api/tenants/onboarding/draft`, `GET /api/tenants/cnpj/{cnpj}/status`, `GET /api/tenants/cep/{cep}`, `POST /api/tenants/onboarding`.
  - BDD: `tests/LivingDoc/Features/Fase2_2_1_TenantOnboarding.feature`.
  - Testes: unitários (`AdministradoraTests`, `CondominioTests`, `CnpjLookupServiceTests`), arquitetura (`TenantOnboardingArchitectureTests`), integração (`TenantOnboardingIntegrationTests`) — Green (122 testes totais da solução).
  - Infra Testing: `InMemoryCacheService` para rascunhos sem Redis local (`Infrastructure:UseInMemoryCache`).
  - Blazor: página `/onboarding` wizard 6 etapas + `OnboardingApiClient` + `onboarding.css` (tokens `#2E5B88`, `#A3C9A8`, `#F4EAE1`); CTA `/sem-tenant` → `/onboarding`.
  - Design Stitch: referência HTML em `stitch_assets/onboarding/wizard-reference.html`.
* [x] **Subfase 2.2.2:** Entidade `Bloco`, `Unidade` (Apartamentos/Casas) e mapeamento dos tipos de moradores (Proprietário / Inquilino).

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Entidades `Bloco`, `Unidade`, `Morador`, `VinculoUnidade`, enums `PapelVinculo`/`UnidadeStatus` e `CpfValidator` em `Modules.Identity.Domain`.
  - EF Core: configs + migration `AddBlocoUnidadeVinculo`; query filters por tenant/condo em `IdentityDbContext`; seed demo (Blocos A/B, unidade 102).
  - Serviço `UnitResidentService` + `IUnitResidentService` com CRUD, filtros, troca de titularidade, histórico e importação XLSX (ClosedXML).
  - FastEndpoints `Result<T>`: `/api/blocks`, `/api/units`, `/api/units/{id}/transfer`, `/api/units/{id}/history`, `/api/units/import/*`.
  - BDD: `tests/LivingDoc/Features/Fase2_2_2_BlocosUnidadesMoradores.feature`.
  - Testes: unitários (`BlocoTests`, `UnidadeTests`, `VinculoUnidadeTests`, `CpfValidatorTests`), arquitetura (`UnitResidentArchitectureTests`), integração (`UnitResidentIntegrationTests`) — Green.
  - Blazor: página `/unidades` com filter bar, tabela, drawer lateral, wizard importação, modal troca titularidade, timeline histórico, painel E2E + `units.css` (tokens `#2E5B88`, `#A3C9A8`, `#F4EAE1`).
  - Design Stitch: referência HTML em `stitch_assets/units/management-reference.html`.


* [x] **Subfase 2.2.3:** [FN-ID-04] Cadastro do número do celular do morador para autenticação nativa via WhatsApp.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio: `PhoneNumberValidator` para E.164 BR, enum `PhoneVerificationStatus` e transições de verificação em `Morador`.
  - EF Core: migration `AddMoradorPhoneVerification`, backfill seguro e índice único filtrado por tenant/condomínio para números validados.
  - Serviço `PhoneVerificationService`: OTP de 6 dígitos com hash SHA-256, TTL de 5 minutos, cooldown de 60 segundos e limite de 5 envios/15 minutos via `ICacheService`.
  - Abstração `IWhatsAppOtpSender` com stub seguro; integração BSP real permanece reservada para a Fase 6.
  - FastEndpoints `Result<T>`: request-code, verify, resend e status em `/api/residents/{moradorId}/phone/*`, com isolamento multi-tenant.
  - Sincronização com `ApplicationUser.PhoneNumber`/`PhoneNumberConfirmed` quando o morador possui usuário Identity.
  - BDD: `tests/LivingDoc/Features/Fase2_2_3_CadastroCelularWhatsApp.feature`.
  - Testes: domínio/validador, arquitetura, integração HTTP e isolamento tenant — Green.
  - Blazor: página responsiva `/verificar-celular`, máscara BR, OTP em 6 campos, timer, alertas e badges; CTA/status integrado em `/unidades`.
  - Design Stitch: referências desktop/mobile em `stitch_assets/phone/` e tokens Manrope, `#2E5B88`, `#A3C9A8`, `#F4EAE1`.


---

## 💰 Fase 3: Módulo Financeiro e de Cobrança (`Financial`)

> 
> **Objetivo:** Implementar o motor de cálculo financeiro, emissão de boletos/PIX, controle de inadimplência e integração com bancos.
> 
> 

### 3.1. Motor Financeiro e Cobrança

* [x] **Subfase 3.1.1:** [FN-FIN-01] Mapear entidades `Fatura`, `Boleto` e `ItemCobranca`.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Entidades de domínio `Fatura`, `Boleto`, `ItemCobranca` e enums (`StatusFatura`, `StatusBoleto`, `TipoItemCobranca`) com isolamento multi-tenant (`ITenantScoped`).
  - Mapeamentos EF Core 10 e `FinancialDbContext` herdando de `MultiTenantDbContext` com Global Query Filter automático por `TenantId`.
  - Serviço `InvoiceService` e interface `IInvoiceService` implementando a camada de aplicação com envelope padronizado `Result` / `Result<T>`.
  - FastEndpoints na API: `GET /api/financial/invoices`, `GET /api/financial/invoices/{id}`, `POST /api/financial/invoices` e `POST /api/financial/invoices/{id}/cancel`.
  - BDD: `tests/LivingDoc/Features/Fase3_1_1_FaturaBoletoItemCobranca.feature`.
  - Suíte de testes TDD: `FinancialDomainTests`, `FinancialArchitectureTests`, `FinancialIntegrationTests` (com PostgreSQL real via Testcontainers) — 100% aprovados.
  - Blazor UI: Páginas `/financeiro/faturas` e modal de detalhe/breakdown fidelizados ao design Stitch (telas `c42737e5cd7d470cb33958b9c365137a` e `030806297a4443d09003456026e4915d`).

* [x] **Subfase 3.1.2:** [TDD] Criar testes unitários para o cálculo de multas, juros pró-rata e desconto até o vencimento.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio: Value Objects `ParametrosCalculoFinanceiro`, `CalculoFinanceiroResultado` e Domain Service `CalculadoraFinanceira` determinístico com suporte a multa (2%), juros pró-rata dia (1% a.m.) e desconto de pontualidade com formatação `pt-BR`.
  - Atualização da entidade `Fatura` com método `AplicarCalculoAtualizado`.
  - Serviço de Aplicação: `FinancialCalculationService` + `IFinancialCalculationService` com Result Pattern (`Result<T>`) e isolamento multi-tenant (`ITenantScoped`).
  - FastEndpoints API: `POST /api/financial/simulator/calculate`, `POST /api/financial/invoices/{id}/simulate` e `GET /api/financial/invoices/{id}/projection`.
  - BDD: `tests/LivingDoc/Features/Fase3_1_2_CalculoMultaJurosDesconto.feature`.
  - Suíte de testes TDD: `FinancialCalculationDomainTests`, `FinancialCalculationArchitectureTests` e `FinancialCalculationIntegrationTests` (com PostgreSQL real via Testcontainers) — 100% aprovados.
  - Blazor UI: Página `/financeiro/simulador` com sliders de parâmetros, KPI Cards, memória de cálculo textual (trilha de auditoria) e tabela de projeções futuras fidelizada ao design do Stitch (telas `bcfc4ae7656347469cd286ab1a22af4c` e `6110ad872ff743eda6d65708b4c49944`) + `simulator.css` com tokens da marca (`#2E5B88`, `#A3C9A8`, `#F4EAE1`).
  - Navegação: Atualização de `NavMenuItems.cs` e `NavMenu.razor` integrando a rota `/financeiro/simulador`.



* [x] **Subfase 3.1.3:** Integração com Gateway de Pagamento/PIX (Ex: Asaas, Juno/Ebanx ou Conta Simples).

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio e Mapeamento: Atualização da entidade `Boleto` com propriedades de cobrança externa (`ExternalChargeId`, `GatewayProvider`, `PixQrCodeBase64`, `DataUltimaSincronizacaoGateway`) e enums `PaymentGatewayProvider` e `GatewayChargeStatus`.
  - Abstração & Infraestrutura: Interface `IPaymentGatewayService`, `AsaasPaymentGatewayService` (integração REST API v3 do Asaas) e `MockPaymentGatewayService` para ambiente dev/testes sem chaves externas.
  - Idempotência & Webhooks: `PaymentWebhookService` com validação de token de acesso (`X-Asaas-Access-Token`), idempotência garantida via Redis cache (`ICacheService`) e conciliação automática de faturas pagas/canceladas.
  - Serviço de Aplicação: `InvoicePaymentApplicationService` + `IInvoicePaymentApplicationService` encapsulado com Result Pattern (`Result<T>`) e isolamento multi-tenant (`ITenantScoped`).
  - FastEndpoints API: `POST /api/financial/invoices/{id}/generate-payment`, `GET /api/financial/invoices/{id}/payment-info`, `POST /api/financial/invoices/{id}/sync-payment` e `POST /api/financial/webhooks/asaas`.
  - BDD: `tests/LivingDoc/Features/Fase3_1_3_IntegracaoGatewayPagamentoPix.feature`.
  - Suíte de testes TDD: `PaymentGatewayDomainTests`, `PaymentGatewayArchitectureTests` e `PaymentGatewayIntegrationTests` (com PostgreSQL e Redis real via Testcontainers) — 100% aprovados.
  - Blazor UI (Stitch): Modal/Drawer `PaymentModal.razor` integrado em `/financeiro/faturas` com abas para QR Code PIX visual em Base64, chave Copia e Cola, Linha Digitável, download de PDF e acionamento de sincronização manual em tempo real.



### 3.2. Controle de Inadimplência e Prestação de Contas

* [x] **Subfase 3.2.1:** [FN-FIN-02] Mapeamento de acordos de renegociação e serviço de régua de inadimplência.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio: Aggregate Root `Acordo`, entidades `ParcelaAcordo`, `AcordoFaturaVinculada`, `EtapaReguaInadimplencia` e `HistoricoCobranca` com contrato `ITenantScoped`.
  - Enums e Value Objects: `StatusAcordo`, `StatusParcelaAcordo`, `CanalCobranca`, `TipoAcaoCobranca`, adição de `StatusFatura.EmAcordo`, `ParametrosSimulacaoAcordo` e `ResumoAcordoCalculado`.
  - Domain Services: `CalculadoraAcordoDomainService` (para divisão exata de parcelas e descontos) e `ReguaInadimplenciaEngine` (para avaliação automatizada de faturas elegíveis por DPD).
  - EF Core 10: Mapeamento em `FinancialDbContext` com Global Query Filter por `TenantId` e `CondoId`.
  - Serviços de Aplicação: `AcordoApplicationService` e `ReguaInadimplenciaAppService` encapsulados com Result Pattern (`Result<T>`).
  - FastEndpoints API: `/api/financial/agreements/*` (simular, criar, listar, obter por ID, cancelar e pagar parcela) e `/api/financial/dunning/*` (configurar régua, processar cobrança em lote e obter dashboard aging list).
  - BDD: `tests/LivingDoc/Features/Fase3_2_1_AcordosReguaInadimplencia.feature`.
  - Suíte de testes TDD: `AcordoDomainTests`, `ReguaInadimplenciaDomainTests`, `FinancialAgreementArchitectureTests` e `FinancialAgreementIntegrationTests` — 100% aprovados.
  - Blazor UI (Stitch): Páginas `/financeiro/acordos` (com modal wizard de criação/simulação) e `/financeiro/inadimplencia` (com dashboard Aging List 30/60/90+ dias, régua D+N e histórico) + estilos `agreements.css` e `dunning.css` com tokens da marca (`#2E5B88`, `#A3C9A8`, `#F4EAE1`).


* [x] **Subfase 3.2.2:** [FN-FIN-03 / FN-FIN-04] Geração de Pastas Digitais de Prestação de Contas, Conciliação Bancária e Relatórios Consolidados Multicondomínio.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio: Entidades `PastaDigital` (Aggregate Root), `DocumentoPrestacaoContas`, `ItemBalancete`, `ContaBancaria`, `ExtratoBancarioItem` e `ConciliacaoBancariaRecord` com suporte `ITenantScoped`.
  - Enums e Value Objects: `StatusPastaDigital`, `CategoriaDocumentoPrestacao`, `CategoriaPlanoContas`, `TipoLancamentoBalancete`, `StatusConciliacaoBancaria`, `TipoTransacaoBancaria`, `TipoContaBancaria` e `OrigemConciliacao`.
  - Domain Services: `PastaDigitalDomainService` (consolidação de balancete mensal e saldos) e `ConciliacaoBancariaDomainService` (motor de score e pareamento automático extrato vs lançamentos).
  - EF Core 10: Mapeamento em `FinancialDbContext` com Global Query Filter por `TenantId`.
  - Serviços de Aplicação: `PastaDigitalApplicationService`, `ConciliacaoBancariaApplicationService` e `RelatorioConsolidadoApplicationService` encapsulados com Result Pattern (`Result<T>`).
  - FastEndpoints API: `/api/financial/digital-binders/*` (criar, listar, obter por ID, itens, documentos, submeter e aprovar/rejeitar), `/api/financial/bank-reconciliation/*` (contas, importar extrato, auto-conciliar, itens pendentes e conciliar manual) e `/api/financial/reports/multi-condo-summary`.
  - BDD: `tests/LivingDoc/Features/Fase3_2_2_PrestacaoContasConciliacaoBancaria.feature`.
  - Suíte de testes TDD: `PastaDigitalDomainTests`, `ConciliacaoBancariaDomainTests`, `FinancialDigitalBinderArchitectureTests` e `FinancialDigitalBinderIntegrationTests` (com PostgreSQL real via Testcontainers) — 100% aprovados.
  - Blazor UI (Stitch): Páginas `/financeiro/prestacao-contas` (gerenciamento e aprovação de pastas digitais), `/financeiro/conciliacao` (interface dual-pane para extrato vs lançamentos) e `/financeiro/relatorios` (dashboard multicondomínio para administradoras) + estilos `digital-binder.css` fiéis aos tokens da marca (`#2E5B88`, `#A3C9A8`, `#F4EAE1`).




---

## 🏢 Fase 4: Módulo de Operações e Instalações (`Operations`)

> 
> **Objetivo:** Regras de uso de áreas comuns, registros de chamados e manutenções preventivas do prédio.
> 
> 

### 4.1. Reservas de Áreas Comuns

* [x] **Subfase 4.1.1:** Entidade `AreaComum` (Salão de Festas, Churrasqueira) com regras de capacidade e custos.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio: Entidade Aggregate Root `AreaComum` e enums `TipoAreaComum` e `StatusAreaComum` com isolamento multi-tenant (`ITenantScoped`), validações de invariantes (capacidade $> 0$, taxas $\ge 0$, horários válidos e antecedência minima/máxima em dias) e propriedade calculada `CustoTotalReserva`.
  - EF Core 10: Mapeamento em `OperationsDbContext` (herdando de `MultiTenantDbContext`) com Global Query Filter automático por `TenantId` e tabela `operations."AreasComuns"`.
  - Serviço de Aplicação: `AreaComumApplicationService` e `IAreaComumApplicationService` encapsulados com Result Pattern (`Result<T>`) e isolamento multi-tenant (`ITenantScoped`).
  - FastEndpoints API: `/api/operations/common-areas` (`POST` criar, `GET` listar com filtros por tipo/status, `GET /summary` resumo KPI, `GET /{id}` por ID, `PUT /{id}` atualizar e `PATCH /{id}/status` alterar status operacional).
  - BDD: `tests/LivingDoc/Features/Fase4_1_1_AreaComumCapacidadeCustos.feature`.
  - Suíte de testes TDD: `AreaComumDomainTests`, `OperationsArchitectureTests` e `AreaComumIntegrationTests` (com PostgreSQL real via Testcontainers) — 100% aprovados.
  - Blazor UI (Stitch): Páginas `/operacoes/areas-comuns` (com KPI cards, grid interativo com badges de status, modal wizard de cadastro/edição e alteração rápida de status) + estilos `common-areas.css` fiéis aos tokens da marca (`#2E5B88`, `#A3C9A8`, `#F4EAE1`).


* [x] **Subfase 4.1.2:** [TDD] Criar regras de domínio para impedir colisão de reservas simultâneas no mesmo horário (usando Redis Distributed Locks para simultaneidade).

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio: Entidade `Reserva`, enum `StatusReserva`, exceção `BookingCollisionException` com isolamento multi-tenant (`ITenantScoped`), validações de antecedência mínima/máxima, limites de capacidade da área comum, cálculo do custo total e verificação de sobreposição temporal (`Overlaps`).
  - Redis Distributed Lock & Isolation: Inclusão do `IDistributedLockService` (`RedisDistributedLockService` e fallback `InMemoryDistributedLockService`) no serviço de aplicação `ReservaApplicationService` com lock atômico por chave granular (`lock:operations:tenant:{tenantId}:areacomum:{areaId}:date:{yyyyMMdd}`).
  - EF Core 10 & Persistence: Mapeamento em `OperationsDbContext` (tabela `operations."Reservas"`), índices compostos de performance para rápida detecção de sobreposição e migration `20260806151900_AddReservaAreaComum`.
  - FastEndpoints API & Result Pattern: Endpoints `/api/operations/reservations` (`POST` criar com status 201/409 Conflict, `GET` listar com filtros, `GET /{id}` obter por ID, `PATCH /{id}/cancel` cancelar, `PATCH /{id}/approve` aprovar, `GET /summary` resumo KPI e `GET /calendar` calendário de ocupação).
  - LivingDoc & TDD: Especificação em Gherkin `tests/LivingDoc/Features/Fase4_1_2_ReservaAreaComumColisaoRedisLock.feature`, suíte unitária `ReservaDomainTests` e testes de integração com Testcontainers (`ReservaIntegrationTests` em PostgreSQL 17 + Redis 7.4 real) — 100% aprovados.
  - Blazor UI (Stitch): Página `/operacoes/reservas` (com KPI cards de ocupação, barra de filtros interativa, tabela com badges de status, modal wizard de solicitação com cálculo em tempo real e alerta visual de colisão) + estilos `reservations.css` fiéis aos tokens da marca (`#2E5B88`, `#A3C9A8`, `#F4EAE1`).




### 4.2. Chamados, Ocorrências e Manutenção Preventiva

* [x] **Subfase 4.2.1:** [FN-OPE-03] Entidade `Ocorrencia` para abertura, upload de fotos e mudança de ciclo de vida do chamado.

  **Status:** ✅ Concluída

  **Entregáveis implementados:**
  - Domínio & Máquina de Estados: Entidade Aggregate Root `Ocorrencia`, entidades `AnexoOcorrencia` e `HistoricoOcorrencia`, exceção `InvalidOcorrenciaStatusTransitionException` e enums `CategoriaOcorrencia`, `PrioridadeOcorrencia` e `StatusOcorrencia` com isolamento multi-tenant (`ITenantScoped`), validações de invariantes de abertura, inclusão de anexos de fotos e máquina de estados para transição do ciclo de vida com rastreabilidade de histórico (linha do tempo).
  - EF Core 10 & Persistence: Mapeamentos Fluent API em `OperationsDbContext` (`operations."Ocorrencias"`, `operations."AnexosOcorrencia"` e `operations."HistoricoOcorrencias"`), índices compostos por `(TenantId, CondoId, Status)` e migration `20260806164000_AddOcorrenciaCicloVidaAnexos`.
  - Serviço de Aplicação: `OcorrenciaApplicationService` e `IOcorrenciaApplicationService` encapsulados com Result Pattern (`Result<T>`) e isolamento multi-tenant (`ITenantScoped`).
  - FastEndpoints API: Endpoints sob `/api/operations/tickets` (`POST` criar chamado com fotos, `GET` listar com filtros por categoria/status/prioridade/morador, `GET /{id}` obter detalhes com fotos e linha do tempo, `PATCH /{id}/status` transicionar ciclo de vida, `POST /{id}/attachments` anexar foto e `GET /summary` resumo KPI de chamados).
  - LivingDoc & TDD: Especificação em Gherkin `tests/LivingDoc/Features/Fase4_2_1_OcorrenciaCicloVidaAnexos.feature`, suíte unitária `OcorrenciaDomainTests`, testes de arquitetura `OperationsArchitectureTests` e testes de integração com Testcontainers (`OcorrenciaIntegrationTests` em PostgreSQL 17 real) — 100% aprovados.
  - Blazor UI (Stitch): Páginas `/operacoes/ocorrencias` (com KPI cards, tabela interativa com badges de status e prioridade, modal wizard de criação com preview de upload `NewTicketModal.razor` e drawer lateral `TicketDetailDrawer.razor` com galeria de fotos e linha do tempo interativa) + estilos `tickets.css` fiéis aos tokens da marca (`#2E5B88`, `#A3C9A8`, `#F4EAE1`).
  - Navegação: Atualização de `NavMenuItems.cs` integrando a rota `/operacoes/ocorrencias`.


* [] **Subfase 4.2.2:** [FN-OPE-04] Calendário de `PlanoManutencao` para alertas automáticos de elevadores, bombas e para-raios.


* [] **Subfase 4.2.3:** [FN-OPE-05] Módulo de Módulos de `AssembleiaVirtual` (Pautas, Votação e Ata).



---

## 🚪 Fase 5: Módulo de Controle de Acesso e Portaria (`AccessControl`)

> 
> **Objetivo:** Registro de visitantes, encomendas e comunicação operacional de entrada.
> 
> 

* [] **Subfase 5.1:** [FN-ACC-01] Cadastro e controle de fluxo de `Visitante` e `PrestadorServico`.


* [] **Subfase 5.2:** [FN-ACC-02] Módulo de `Encomendas` (Registro de recebimento pela portaria e baixa pelo morador).



---

## 💬 Fase 6: Engine de WhatsApp e Ingestão Assíncrona

> 
> **Objetivo:** Receber Webhooks de plataformas de WhatsApp com altíssima vazão sem travar a aplicação.
> 
> 

* [] **Subfase 6.1:** [FN-WPP-01] Endpoint de Webhook para recepção de payloads de mensagem do WhatsApp API (Twilio, Z-API, Evolution API).


* [] **Subfase 6.2:** Publicação imediata do payload no RabbitMQ via MassTransit com resposta HTTP `200 OK` instantânea para o gateway.


* [] **Subfase 6.3:** Consumidor em Background (`WhatsAppInboundConsumer`) para extrair telefone, texto/mídia e resolver o `tenant_id` e `morador_id` no Redis/Postgres.



---

## 🤖 Fase 7: AI Orchestrator & Integrations (Semantic Kernel)

> 
> **Objetivo:** Dotar o sistema de inteligência, permitindo ao morador executar funções do sistema por conversa em linguagem natural.
> 
> 

### 7.1. Configuração do Semantic Kernel & RAG

* [] **Subfase 7.1.1:** Setup do Microsoft.SemanticKernel integrado ao OpenAI / Azure OpenAI.


* [] **Subfase 7.1.2:** [FN-OPE-IA03] Criação do pipeline de RAG: Leitura da Convenção/Regimento Interno, geração de embeddings e busca por similaridade vetorial via **pgvector**.



### 7.2. Plugins e Function Calling para o Agente

* [] **Subfase 7.2.1:** [FN-FIN-IA01] Plugin `GetPendingBoletos(moradorId)` -> Retorna chave PIX Copia e Cola e link de PDF do boleto em aberto.


* [] **Subfase 7.2.2:** [FN-OPE-IA01] Plugin `ReserveCommonArea(areaId, data, moradorId)` -> Valida e agenda área comum pelo chat.


* [] **Subfase 7.2.3:** [FN-ACC-IA01] Plugin `AuthorizeGuest(nome, documento, data)` -> Registra liberação de visitante na portaria.


* [] **Subfase 7.2.4:** [FN-ACC-IA02] Leitura inteligente de etiquetas de encomenda via Vision/OCR + IA e notificação automática.


* [] **Subfase 7.2.5:** [FN-OPE-IA02] Triagem de ocorrências enviadas por foto/áudio e abertura automática de chamados direcionados.



---

## 🧪 Fase 8: Testes BDD (Living Documentation), QA & CI/CD

> 
> **Objetivo:** Executar a bateria de validação ponta a ponta e garantir a qualidade do software.
> 
> 

* [] **Subfase 8.1:** Escrita e execução das suítes de BDD em Português usando **Reqnroll / SpecFlow**.


* [] **Subfase 8.2:** Rodar suíte de testes de integração com **Testcontainers** instanciando instâncias reais de Postgres com Pgvector e Redis.


* [] **Subfase 8.3:** Configuração do GitHub Actions / GitLab CI para validação de build, testes automatizados e publicação de relatórios HTML de documentação viva.
