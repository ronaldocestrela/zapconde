# language: pt-BR
Funcionalidade: Setup de persistencia relacional com EF Core 10 e PostgreSQL

  Como time tecnico da plataforma SmartCondo
  Quero configurar a base de persistencia no BuildingBlocks.Infrastructure
  Para habilitar evolucao segura para multi-tenancy nas subfases seguintes

  Cenario: Projeto Infrastructure referencia EF Core e Npgsql
    Dado que o projeto BuildingBlocks.Infrastructure foi configurado para persistencia relacional
    Quando eu inspecionar as dependencias do projeto
    Entao deve existir referencia ao pacote "Microsoft.EntityFrameworkCore"
    E deve existir referencia ao pacote "Npgsql.EntityFrameworkCore.PostgreSQL"

  Cenario: API possui connection string base para PostgreSQL
    Dado que a API SmartCondo utiliza configuracao por appsettings
    Quando eu consultar a chave "ConnectionStrings:Postgres"
    Entao a connection string deve estar definida no arquivo base
    E deve permitir override por ambiente de desenvolvimento

  Cenario: Conectividade basica com PostgreSQL em ambiente de teste
    Dado que um container PostgreSQL de teste esta em execucao
    Quando uma conexao for aberta usando a connection string do container
    Entao a conexao deve ser estabelecida com sucesso
