# language: pt-BR
Funcionalidade: Transactional Outbox Pattern com EF Core e MassTransit para Idempotencia

  Como desenvolvedor da plataforma SmartCondo
  Quero habilitar o Transactional Outbox Pattern no EF Core e MassTransit
  Para garantir atomicidade entre alteracoes de banco de dados e envio de mensagens assincronas

  Contexto:
    Dado que a solucao possui infraestrutura com EF Core 10, PostgreSQL e MassTransit
    E a subfase 1.3.2 de idempotencia e outbox pattern esta em implementacao

  Cenario: Dependencia do pacote MassTransit.EntityFrameworkCore esta presente
    Quando eu inspecionar as dependencias do projeto BuildingBlocks.Infrastructure
    Entao deve existir a referencia ao pacote "MassTransit.EntityFrameworkCore" com versao compativel

  Cenario: Tabelas do Outbox sao mapeadas no DbContext sem violar filtro global de tenant
    Dado que o MultiTenantDbContext e o contexto base de persistencia
    Quando o modelo EF Core for construído no OnModelCreating
    Entao as entidades de Outbox e Inbox do MassTransit devem ser registradas
    E essas entidades nao devem ser afetadas pelo filtro global ITenantScoped

  Cenario: MassTransit e registrado com suporte a Outbox no EF Core
    Dado que a infraestrutura registra os servicos via AddInfrastructure
    Quando a configuracao do MassTransit for executada
    Entao o bus deve habilitar AddEntityFrameworkOutbox com provedor Postgres
    E deve utilizar UseBusOutbox para publicar mensagens via transacao de banco de dados

  Cenario: Publicacao de evento e alteracao de dominio ocorrem na mesma transacao
    Dado que um contexto de banco de dados possui suporte a Outbox habilitado
    Quando uma alteracao de entidade for salva e um evento for publicado no mesmo DbContext
    Entao a mensagem do evento deve ser gravada na tabela de outbox no mesmo commit
    E a mensagem deve ser posteriormente despachada ao broker RabbitMQ de forma assincrona

  Cenario: Testes de integracao validam Outbox com PostgreSQL e RabbitMQ via Testcontainers
    Dado que a suíte de testes de integracao executa a subfase 1.3.2
    Quando os conteineres de PostgreSQL e RabbitMQ estiverem ativos
    Entao o ciclo de vida completo do Outbox Pattern deve ser validado com persistencia real
