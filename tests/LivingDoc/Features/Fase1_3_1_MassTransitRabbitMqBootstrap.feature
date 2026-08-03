# language: pt-BR
Funcionalidade: Bootstrap de mensageria assincrona com MassTransit e RabbitMQ

  Como time tecnico da plataforma SmartCondo
  Quero configurar MassTransit com RabbitMQ na infraestrutura
  Para habilitar processamento orientado a eventos com observabilidade de saude do broker

  Contexto:
    Dado que a solucao utiliza .NET 10 e arquitetura modular monolith
    E a composicao de infraestrutura ocorre via AddInfrastructure

  Cenario: Projeto Infrastructure referencia MassTransit e transporte RabbitMQ
    Dado que a subfase 1.3.1 foi iniciada
    Quando eu inspecionar as dependencias do projeto BuildingBlocks.Infrastructure
    Entao deve existir referencia ao pacote "MassTransit"
    E deve existir referencia ao pacote "MassTransit.RabbitMQ"

  Cenario: Configuracao dedicada de RabbitMQ existe no appsettings
    Dado que a API possui configuracoes de ambiente
    Quando eu ler os arquivos appsettings da API
    Entao deve existir a secao "RabbitMQ"
    E a secao deve conter Host, Port, VirtualHost, Username e Password

  Cenario: Bootstrap valida configuracao obrigatoria do broker
    Dado que AddInfrastructure recebe IConfiguration
    Quando a secao RabbitMQ estiver ausente ou invalida
    Entao deve ser lancada InvalidOperationException
    E a aplicacao nao deve iniciar com configuracao incompleta

  Cenario: MassTransit e configurado com transporte RabbitMQ
    Dado que a secao RabbitMQ esta valida
    Quando AddInfrastructure registrar os servicos
    Entao o bus do MassTransit deve ser configurado para usar RabbitMQ
    E a aplicacao deve manter composicao centralizada na camada Infrastructure

  Cenario: Readiness do broker e exposta por health check
    Dado que o bus do MassTransit esta registrado
    Quando a API iniciar o pipeline HTTP
    Entao o endpoint "/health/ready" deve estar disponivel
    E deve incluir verificacao explicita de saude do RabbitMQ
    E o endpoint funcional "/api/health" deve permanecer sem conflito

  Cenario: Testes de integracao usam RabbitMQ real
    Dado que a subfase 1.3.1 exige validacao real de broker
    Quando os testes de integracao executarem
    Entao deve ser utilizado Testcontainers.RabbitMq
    E a aplicacao deve conseguir inicializar com as credenciais do container
