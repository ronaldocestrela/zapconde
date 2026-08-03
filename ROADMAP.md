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


* [] **Subfase 1.3.2:** Habilitar o **Transactional Outbox Pattern** no EF Core/MassTransit para garantir idempotência.


* [] **Subfase 1.3.3:** Configurar cliente Redis (`StackExchange.Redis`) para controle de cache, Distributed Locks e sessão de chat.



---

## 🔐 Fase 2: Módulo Identity, Autenticação e Cadastro Base

> 
> **Objetivo:** Garantir a autenticação de usuários, mapear a estrutura física do condomínio e vincular os números de celular ao WhatsApp.
> 
> 

### 2.1. ASP.NET Core Identity e JWT

* [] **Subfase 2.1.1:** Mapear e aplicar tabelas do Identity customizadas (gerando JWT com claims obrigatórias: `TenantId`, `CondoId`, `UserId`, `Role`).


* [] **Subfase 2.1.2:** Criar Middleware de Injeção de Contexto (`CurrentTenantService`) para extrair o `TenantId` do Token JWT ou do Header do Webhook.



### 2.2. Cadastro de Domínio Condominial

* [] **Subfase 2.2.1:** Entidade `Condominio` e `Administradora` (Criação de Tenant).
* [] **Subfase 2.2.2:** Entidade `Bloco`, `Unidade` (Apartamentos/Casas) e mapeamento dos tipos de moradores (Proprietário / Inquilino).


* [] **Subfase 2.2.3:** [FN-ID-04] Cadastro do número do celular do morador para autenticação nativa via WhatsApp.



---

## 💰 Fase 3: Módulo Financeiro e de Cobrança (`Financial`)

> 
> **Objetivo:** Implementar o motor de cálculo financeiro, emissão de boletos/PIX, controle de inadimplência e integração com bancos.
> 
> 

### 3.1. Motor Financeiro e Cobrança

* [] **Subfase 3.1.1:** [FN-FIN-01] Mapear entidades `Fatura`, `Boleto` e `ItemCobranca`.
* [] **Subfase 3.1.2:** [TDD] Criar testes unitários para o cálculo de multas, juros pró-rata e desconto até o vencimento.


* [] **Subfase 3.1.3:** Integração com Gateway de Pagamento/PIX (Ex: Asaas, Juno/Ebanx ou Conta Simples).



### 3.2. Controle de Inadimplência e Prestação de Contas

* [] **Subfase 3.2.1:** [FN-FIN-02] Mapeamento de acordos de renegociação e serviço de régua de inadimplência.


* [] **Subfase 3.2.2:** [FN-FIN-03 / FN-FIN-04] Geração de Pastas Digitais de Prestação de Contas, Conciliação Bancária e Relatórios Consolidados Multicondomínio.



---

## 🏢 Fase 4: Módulo de Operações e Instalações (`Operations`)

> 
> **Objetivo:** Regras de uso de áreas comuns, registros de chamados e manutenções preventivas do prédio.
> 
> 

### 4.1. Reservas de Áreas Comuns

* [] **Subfase 4.1.1:** Entidade `AreaComum` (Salão de Festas, Churrasqueira) com regras de capacidade e custos.


* [] **Subfase 4.1.2:** [TDD] Criar regras de domínio para impedir colisão de reservas simultâneas no mesmo horário (usando Redis Distributed Locks para simultaneidade).



### 4.2. Chamados, Ocorrências e Manutenção Preventiva

* [] **Subfase 4.2.1:** [FN-OPE-03] Entidade `Ocorrencia` para abertura, upload de fotos e mudança de ciclo de vida do chamado.


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
