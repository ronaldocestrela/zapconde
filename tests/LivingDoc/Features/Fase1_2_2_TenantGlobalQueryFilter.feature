# language: pt-BR
Funcionalidade: Isolamento multi-tenant com Global Query Filter automatico

  Como time tecnico da plataforma SmartCondo
  Quero garantir isolamento automatico de dados por tenant_id
  Para proteger a privacidade dos condominios conforme LGPD e arquitetura multi-tenant

  Contexto:
    Dado que a solucao esta configurada com multi-tenancy
    E existe a interface ITenantScoped em BuildingBlocks.Shared
    E existe o servico ICurrentTenantService para resolucao de tenant

  Cenario: Interface ITenantScoped define contrato de tenant
    Dado que uma entidade implementa ITenantScoped
    Quando o modelo do EF Core e analisado
    Entao a entidade deve ter a propriedade TenantId do tipo inteiro
    E o contexto base deve aplicar HasQueryFilter automaticamente

  Cenario: DbContext base aplica filtro apenas em entidades tenant
    Dado que o MultiTenantDbContext foi criado
    E existem entidades que implementam ITenantScoped
    E existem entidades que nao implementam ITenantScoped
    Quando o modelo do EF Core e configurado no OnModelCreating
    Entao apenas as entidades ITenantScoped devem ter query filter aplicado
    E as entidades globais nao devem ter filtro

  Cenario: Consulta retorna vazio quando tenant nao esta resolvido (deny-by-default)
    Dado que o ICurrentTenantService.TenantId retorna null
    E existem registros de multiplos tenants no banco de dados
    Quando uma consulta LINQ e executada
    Entao a consulta deve retornar vazio
    E nenhum dado deve vazar para requisicoes nao autenticadas

  Cenario: Consulta filtra automaticamente por tenant resolvido
    Dado que o ICurrentTenantService.TenantId retorna 1
    E existem registros do tenant 1, tenant 2 e tenant 3 no banco
    Quando uma consulta LINQ e executada em entidades ITenantScoped
    Entao apenas registros com TenantId igual a 1 devem ser retornados
    E registros de outros tenants nao devem aparecer

  Cenario: Isolamento completo entre tenants em operacoes de escrita e leitura
    Dado que o tenant 1 persiste um registro via SaveChanges
    E o tenant 2 persiste seu proprio registro via SaveChanges
    Quando o tenant 1 consulta suas entidades
    Entao deve ver apenas seu proprio registro
    Quando o tenant 2 consulta suas entidades
    Entao deve ver apenas seu proprio registro
    E nenhum tenant deve ter acesso aos dados do outro

  Cenario: Entidades globais nao sao filtradas por tenant
    Dado que existem entidades de configuracao global que nao implementam ITenantScoped
    E o tenant atual e resolvido como tenant 1
    Quando uma consulta LINQ e executada em entidades globais
    Entao todas as entidades globais devem ser retornadas
    E o filtro de tenant nao deve ser aplicado

  Cenario: Arquitetura mantem Clean Architecture com contratos em Shared
    Dado que os contratos de multi-tenancy existem
    Quando a dependencia de assemblies e analisada
    Entao ITenantScoped deve estar em BuildingBlocks.Shared
    E ICurrentTenantService deve estar em BuildingBlocks.Shared
    E BuildingBlocks.Shared nao deve depender de Entity Framework Core
    E BuildingBlocks.Infrastructure deve referenciar BuildingBlocks.Shared

  Cenario: Servico de tenant e registrado como Scoped para isolamento por requisicao
    Dado que a aplicacao esta configurada via DependencyInjection
    Quando os servicos de infraestrutura sao registrados
    Entao ICurrentTenantService deve estar registrado como Scoped
    E cada requisicao HTTP deve ter sua propria instancia do servico
