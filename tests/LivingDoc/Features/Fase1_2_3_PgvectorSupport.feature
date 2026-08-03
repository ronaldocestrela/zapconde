# language: pt-BR
Funcionalidade: Suporte a pgvector no PostgreSQL para busca vetorial (RAG)

  Como time tecnico da plataforma SmartCondo
  Quero habilitar suporte a tipos vetoriais no PostgreSQL com pgvector
  Para viabilizar busca de similaridade semantica futura no modulo AIEngine (RAG)

  Contexto:
    Dado que a solucao esta configurada com EF Core 10 e Npgsql
    E a infraestrutura de persistencia suporta multi-tenancy

  Cenario: Projeto Infrastructure referencia Pgvector.EntityFrameworkCore
    Dado que o projeto BuildingBlocks.Infrastructure foi configurado para persistencia vetorial
    Quando eu inspecionar as dependencias do projeto
    Entao deve existir referencia ao pacote "Pgvector.EntityFrameworkCore" versao 0.3.0
    E o pacote deve estar disponivel para uso em contextos EF Core

  Cenario: PostgreSQL suporta extensao pgvector no container de teste
    Dado que um container PostgreSQL com imagem pgvector/pgvector:pg17 esta em execucao
    Quando eu executar o comando "CREATE EXTENSION IF NOT EXISTS vector"
    Entao a extensao deve ser habilitada com sucesso
    E o tipo "vector" deve estar disponivel para uso em colunas

  Cenario: EF Core mapeia tipo vetorial com pgvector
    Dado que a extensao pgvector foi habilitada no banco de dados
    E um DbContext derivado de MultiTenantDbContext foi configurado com UseVector
    Quando eu definir uma propriedade do tipo Vector em uma entidade
    E aplicar EnsureCreatedAsync para criar as tabelas
    Entao a coluna deve ser criada com o tipo "vector" no PostgreSQL
    E o mapeamento deve permitir persistencia e leitura de embeddings

  Cenario: Persistencia e leitura de embeddings vetoriais
    Dado que a extensao pgvector foi habilitada no banco de dados
    E um DbContext com suporte a vector esta configurado
    Quando eu persistir um documento com embedding vetorial de 3 dimensoes
    E em seguida consultar o documento do banco de dados
    Entao o embedding deve ser recuperado corretamente
    E os valores do vetor devem corresponder aos valores persistidos

  Cenario: Consulta de similaridade vetorial com ordenacao por distancia L2
    Dado que existem multiplos documentos com embeddings vetoriais no banco
    Quando eu executar uma consulta utilizando o metodo L2Distance
    E ordenar os resultados pela distancia crescente
    Entao os documentos devem ser retornados na ordem de similaridade
    E o documento mais similar deve aparecer primeiro na lista
    E a consulta deve respeitar o limite de resultados especificado

  Cenario: Isolamento multi-tenant preservado em embeddings vetoriais
    Dado que o tenant 1 persistiu embeddings de documentos
    E o tenant 2 persistiu seus proprios embeddings de documentos
    Quando o tenant 1 consulta embeddings do banco de dados
    Entao deve ver apenas seus proprios documentos com embeddings
    Quando o tenant 2 consulta embeddings do banco de dados
    Entao deve ver apenas seus proprios documentos com embeddings
    E o filtro global de tenant deve ser aplicado automaticamente

  Cenario: Configuracao reutilizavel de suporte vetorial disponivel
    Dado que a infraestrutura fornece extensoes de configuracao
    Quando um modulo ou contexto precisa habilitar suporte a vector
    Entao deve poder usar o metodo UseNpgsqlWithVector da infraestrutura
    E o metodo deve configurar automaticamente o NpgsqlDataSource com Vector
    E deve ser compativel com a arquitetura multi-tenant existente

  Cenario: Base preparada para implementacao futura de RAG
    Dado que o suporte a pgvector foi configurado na infraestrutura
    E os testes de integracao validaram a funcionalidade
    Quando o modulo AIEngine for implementado no futuro
    Entao podera utilizar tipos vetoriais para armazenar embeddings de documentos
    E podera executar consultas de similaridade semantica para RAG
    E o isolamento multi-tenant sera mantido automaticamente
