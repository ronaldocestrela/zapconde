# `INSTRUCTIONS.md` / `PROJECT_SPEC.md`

```markdown
# 🏛️ System Architecture & Implementation Directive: Smart Condo SaaS (.NET 10)

## 📌 Contexto & Objetivo
Você é um Engenheiro de Software Principal responsável por construir o backend de um **SaaS Multi-tenant para Gestão Condominial Inteligente** com suporte a atendimento via **WhatsApp Business API** e **Agentes de IA (RAG + Function Calling)**.

[cite_start]O sistema atende **Administradoras, Síndicos e Condôminos**[cite: 1, 14, 21]. Toda a aplicação deve ser desenvolvida em **.NET 10**, seguindo princípios de **Clean Architecture**, **Domain-Driven Design (DDD)**, **Test-Driven Development (TDD)** e **Documentação Viva**.

O projeto deve seguir o design do Stitch: `https://stitch.withgoogle.com/projects/13499280582130189383`.

---

## 🛠️ Stack Tecnológica Obrigatória

* **Framework Principal:** .NET 10 (C# 14)
* **API Framework:** ASP.NET Core Web API (Minimal APIs + FastEndpoints)
* **Persistência de Dados (Relacional):** Entity Framework Core 10 + PostgreSQL (com Row-Level Security para Multi-tenancy)
* **Banco Vetorial (RAG):** Pgvector extension no PostgreSQL (via `Pgvector.EntityFrameworkCore`) ou Qdrant
* **Identity & Autenticação:** ASP.NET Core Identity + Duende IdentityServer ou OpenIddict (JWT com claims customizadas de `tenant_id` e `role`)
* **Filas & Mensageria:** MassTransit + RabbitMQ
* **Cache em Memória & Distributed Lock:** Redis (via StackExchange.Redis)
* **Integração IA:** Microsoft.SemanticKernel / LangChain.NET + OpenAI API / Azure OpenAI SDK
* **Testes:** xUnit, FluentAssertions, Moq, Testcontainers (para testes de integração com Postgres e Redis reais)
* **Documentação Viva:** OpenAPI / Scalar / Swashbuckle + LivingDoc (SpecFlow / Reqnroll para BDD)

---

## 🏗️ Arquitetura do Projeto (Clean Architecture + Modular Monolith)

[cite_start]A solução deve seguir uma estrutura modular preparada para transição para microsserviços, caso necessário[cite: 56]:

```text
src/
├── BuildingBlocks/
│   ├── BuildingBlocks.Domain/       # Entidades base, Value Objects, Domain Events
│   ├── BuildingBlocks.Infrastructure/# Interceptors (Audit, Tenant), Abstrações de Bus/Cache
│   └── BuildingBlocks.Shared/       # DTOs compartilhados e Result Pattern
├── Modules/
│   ├── Identity/                    # ASP.NET Core Identity, Roles, Auth Tokens
│   ├── Financial/                   # Boletos, PIX, Inadimplência, Conciliação
│   ├── Operations/                  # Áreas Comuns, Reservas, Ocorrências, Manutenção
│   ├── AccessControl/               # Visitantes, Encomendas, Portaria
│   ├── WhatsApp/                    # Webhooks, Filas de envio, Provedor BSP
│   └── AIEngine/                    # Orchestrator, Semantic Kernel, RAG, Embeddings
└── API/
    └── SmartCondo.Api/              # Host principal, Entrypoint, Middlewares, Gateways

```

---

## 🔑 Requisitos Arquiteturais Críticos

### 1. Isolation & Multi-Tenancy (LGPD & Segurança)

* 
**Model:** Multi-tenant usando tabela compartilhada com isolamento obrigatório por `tenant_id`.


* **EF Core Global Query Filter:** Toda entidade com a interface `ITenantScoped` DEVE aplicar automaticamente o filtro:
```csharp
builder.Entity<TEntity>().HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);

```


* 
**Identity:** Claims de acesso devem conter `TenantId`, `CondoId`, `UserId` e `Role` (Admin, Síndico, Zelador, Condômino).



### 2. Mensageria & Event-Driven Architecture

* 
**Resiliência no WhatsApp:** O recebimento de Webhooks do WhatsApp Business API deve ser assíncrono.


* 
**Flow:** `Webhook` -> `Minimal API` -> `Publish MassTransit Event` -> `Queue (RabbitMQ)` -> `Background Consumer (AI Orchestrator)`.


* **Idempotência:** Utilize o **Outbox Pattern** e Redis para garantir que a mesma mensagem de WhatsApp não seja processada duas vezes.

### 3. Engine de IA & RAG

* Implemente o **Semantic Kernel** (.NET 10) para orquestrar chamadas de LLM.


* 
**RAG (Regimento Interno/Convenções):** Utilize busca vetorial com **pgvector** para responder dúvidas operacionais dos moradores.


* **Function Calling (Tools):** O agente de IA deve ser capaz de invocar funções locais fortemente tipadas para:
1. 
`GetPendingBoletos(moradorId)` -> Retorna código PIX e PDF.


2. 
`ReserveCommonArea(areaId, data, moradorId)` -> Checa disponibilidade e efetua reserva.


3. 
`AuthorizeGuest(nome, documento, data)` -> Cadastra portaria.





---

## 🧪 Práticas de Desenvolvimento: TDD & Documentação Viva

### 1. Test-Driven Development (TDD) - Diretrizes

**Todo código funcional DEVE ser escrito após a criação dos testes que falham (Red-Green-Refactor).**

* 
**Testes Unitários:** Cobrir obrigatoriamente REGRAS DE NEGÓCIO de domínios (ex: cálculo de juros/multa de boletos , validação de colisão de horários em reservas ).


* 
**Testes de Integração:** Utilizar **Testcontainers** para instanciar bancos Postgres e Redis reais durante a execução dos testes de repositório e serviços.


* **Exemplo de Teste de Domínio (Reserva de Áreas Comuns):**
```csharp
[Fact]
public void Should_ThrowException_When_ReservationCollidesWithExistingBooking()
{
    // Arrange
    var area = AreaComum.Create("Salão de Festas");
    var existingBooking = area.AddBooking(DateTime.Today.AddHours(18), DateTime.Today.AddHours(22), tenantId: 1);

    // Act & Assert
    Assert.Throws<BookingCollisionException>(() => 
        area.AddBooking(DateTime.Today.AddHours(20), DateTime.Today.AddHours(23), tenantId: 1));
}

```



### 2. Documentação Viva (Living Documentation)

* Utilizar **Reqnroll / SpecFlow** para criar cenários BDD escritos em Gherkin (`.feature`) na linguagem nativa dos stakeholders (Português).
* Os arquivos `.feature` devem ser executados via pipeline e gerar relatórios HTML automatizados no build.

**Exemplo de Arquivo BDD (`BoletosWhatsApp.feature`):**

```gherkin
# language: pt-BR
Funcionalidade: Solicitacao de Segunda Via de Boleto via WhatsApp

  Cenario: Morador solicita boleto e recebe chave PIX e PDF em segundos
    Dado que o morador com telefone "+5575999999999" está cadastrado no condomínio "1"
    E possui uma fatura em aberto no valor de "250.00"
    Quando o WhatsApp Webhook recebe a mensagem "Pode me mandar o boleto deste mês?"
    Então a IA deve identificar a intenção "GET_BOLETO"
    E deve disparar a resposta no WhatsApp contendo o código PIX Copia e Cola e o link do PDF

```

---

## ⚙️ Regras de Implementação para o Agente de IA

Ao ser solicitado a implementar qualquer módulo neste repositório, você DEVE seguir estes passos estritamente:

### 🎯 Padrão de Retorno Obrigatório: Result Pattern

**TODA resposta de API DEVE utilizar o tipo `Result` ou `Result<T>` definido em `BuildingBlocks.Shared`.**

* **Proibições:**
  - ❌ Nunca retornar exceções brutas ou stack traces no payload da resposta.
  - ❌ Nunca retornar `null` ou tipos primitivos diretamente sem encapsulamento.
  - ❌ Nunca usar códigos HTTP inconsistentes com o estado de negócio.

* **Estrutura do `Result`:**
  ```csharp
  public class Result
  {
      public bool IsSuccess { get; }
      public string Message { get; }
      public IEnumerable<string> Errors { get; }
      // Métodos factory: Success(), Failure(...), ValidationFailure(...)
  }

  public class Result<T> : Result
  {
      public T Data { get; }
      // Método factory: Success(T data)
  }
  ```

* **Mapeamento Obrigatório para HTTP Status Codes:**
  | Estado de Negócio | HTTP Status | Método `Result` | Exemplo de Cenário |
  |-------------------|-------------|-----------------|-------------------|
  | ✅ Sucesso com dados | `200 OK` | `Result<T>.Success(data)` | Consulta retornou registros |
  | ✅ Criação bem-sucedida | `201 Created` | `Result<T>.Success(data)` | Novo recurso criado com sucesso |
  | ⚠️ Validação falhou | `400 Bad Request` | `Result.ValidationFailure(errors)` | Campos obrigatórios ausentes |
  | ⚠️ Entidade processável mas inválida | `422 Unprocessable Entity` | `Result.ValidationFailure(errors)` | CPF inválido, data no passado |
  | 🚫 Recurso não encontrado | `404 Not Found` | `Result.Failure("Not found")` | Morador ou Boleto inexistente |
  | ⚠️ Conflito de negócio | `409 Conflict` | `Result.Failure("Conflict")` | Reserva já existe no horário |
  | ❌ Erro inesperado do sistema | `500 Internal Server Error` | `Result.Failure("System error")` | Banco fora do ar, exceção não tratada |

* **Exemplo de Uso em Endpoint (FastEndpoints):**
  ```csharp
  public class GetBoletosEndpoint : Endpoint<GetBoletosRequest, Result<IEnumerable<BoletoDto>>>
  {
      public override async Task HandleAsync(GetBoletosRequest req, CancellationToken ct)
      {
          var boletos = await _boletoService.GetPendingBoletosAsync(req.MoradorId, ct);

          if (!boletos.Any())
              await SendAsync(Result<IEnumerable<BoletoDto>>.Success(boletos), 200, ct);
          else
              await SendAsync(Result<IEnumerable<BoletoDto>>.Success(boletos), 200, ct);
      }
  }
  ```

### 🎨 Identidade Visual (Stitch) e Comunicação MCP

* **Identidade Visual Obrigatória:** quando houver implementação de telas, protótipos, fluxos de UX, componentes visuais ou documentação de interface, seguir a identidade visual do projeto Stitch:
  - `https://stitch.withgoogle.com/projects/13499280582130189383`

* **Comunicação via MCP Obrigatória:** para integrações entre agentes, ferramentas e serviços externos, priorizar o uso de MCP (Model Context Protocol) como canal padrão de comunicação, com contratos explícitos, rastreáveis e versionáveis.

---

### 📋 Fluxo de Implementação (TDD + Result Pattern)

1. **Passo 1 (Especificação):** Crie ou leia o arquivo `.feature` correspondente na pasta `/tests/LivingDoc/Features`.
2. **Passo 2 (TDD - Red):** Crie as classes de teste de unidade/integração que validam a funcionalidade. Garanta que elas falhem inicialmente.
3. **Passo 3 (Domain & EF Core):** Crie as entidades de domínio utilizando `Entity Framework Core 10` com mapeamentos em arquivos de configuração (`IEntityTypeConfiguration<T>`).
4. **Passo 4 (Application & CQRS):** Implemente os Handlers (MediatR/FastEndpoints) aplicando os filtros de Multi-tenancy (`tenant_id`) **e retornando obrigatoriamente `Result`/`Result<T>`**.
5. **Passo 5 (TDD - Green):** Faça os testes passarem.
6. **Passo 6 (Refactor & Docs):** Refatore o código garantindo legibilidade, padrões C# 14, e adicione comentários XML em endpoints públicos para geração da documentação OpenAPI.

---

## 📋 Módulos Principais a Serem Implementados

| Módulo | Componentes Chave em .NET 10 |
| --- | --- |
| **Identity & Auth** | ASP.NET Core Identity, JWT, Custom Claims (`tenant_id`), Middleware de Injeção de Contexto. |
| **Financial** | Entidades `Boleto`, `Fatura`, `Acordo`; Serviços de cobrança; Integração de Gateway; Relatórios de Inadimplência.

 |
| **Operations** | Entidades `AreaComum`, `Reserva`, `Ocorrencia`, `PlanoManutencao`.

 |
| **WhatsApp Engine** | Controller do Webhook do WhatsApp, Consumidor MassTransit `WhatsAppInboundConsumer`, formatador de mensagens.

 |
| **AI Orchestrator** | Plugins do Semantic Kernel, Conector com PgVector para RAG do Regimento Interno, Prompt Templates.

 |

---

```

---

### 💡 Dica de Uso
Você pode salvar o conteúdo do bloco de código acima diretamente em um arquivo chamado `AGENTS.md` ou `PROMPT_ENGINEERING.md` na raiz do seu projeto **.NET 10**. Sempre que abrir uma nova sessão com assistentes de IA (como Cursor, Claude Code, etc.), refira-se a este arquivo como as **instruções mestre** da arquitetura do seu sistema!

```
