# language: pt-BR
Funcionalidade: Gestao de Blocos, Unidades e Vinculo de Moradores
  Como administrador ou sindico
  Eu quero cadastrar blocos, unidades e moradores com papel de proprietario ou inquilino
  Para mapear a estrutura fisica e titularidade do condominio

  Contexto:
    Dado que o modulo de unidades esta disponivel na API
    E que estou autenticado com tenantId "1" e condoId "10"

  Cenario: Criacao de unidade com morador retorna 201
    Quando eu envio POST para "/api/units" com payload valido de unidade e morador
    Entao o status HTTP deve ser 201 Created
    E a resposta deve conter "isSuccess" com valor "true"
    E a resposta deve conter unitId e residentId

  Cenario: Unidade duplicada no mesmo bloco retorna 409
    Dado que existe unidade "101" no bloco "Bloco A"
    Quando eu envio POST para "/api/units" com numero "101" no mesmo bloco
    Entao o status HTTP deve ser 409 Conflict

  Cenario: CPF invalido retorna 422
    Quando eu envio POST para "/api/units" com CPF invalido
    Entao o status HTTP deve ser 422 Unprocessable Entity

  Cenario: Listagem com filtros por bloco, status e papel
    Dado que existem unidades cadastradas com diferentes status e papeis
    Quando eu consulto GET "/api/units" com filtro bloco "Bloco A"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter apenas unidades do bloco filtrado

  Cenario: Troca de titularidade arquiva vinculo antigo
    Dado que existe unidade com proprietario ativo
    Quando eu envio POST para "/api/units/{id}/transfer" com dados do novo titular
    Entao o status HTTP deve ser 200 OK
    E o vinculo antigo deve estar encerrado no historico
    E o novo vinculo deve estar ativo

  Cenario: Historico de alteracoes retorna timeline
    Dado que existe unidade com historico de vinculos
    Quando eu consulto GET "/api/units/{id}/history"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter entradas com datas de inicio e fim

  Cenario: Importacao em lote preview valida linhas
    Quando eu envio POST para "/api/units/import/preview" com planilha contendo CPF invalido
    Entao o status HTTP deve ser 200 OK
    E a resposta deve marcar linhas invalidas com erro de CPF

  Cenario: Importacao em lote commit persiste linhas validas
    Quando eu envio POST para "/api/units/import/commit" com linhas validas
    Entao o status HTTP deve ser 200 OK
    E as unidades validas devem ser persistidas

  Cenario: Isolamento multi-tenant impede acesso cross-tenant
    Dado que existe unidade no tenant "2"
    Quando eu consulto GET "/api/units" autenticado no tenant "1"
    Entao a resposta nao deve conter unidades do tenant "2"

  Cenario: UI exibe barra de filtros e tabela de unidades
    Quando acesso a pagina "/unidades"
    Entao devo ver campo de busca "Buscar por unidade, morador ou bloco"
    E devo ver filtros de Bloco, Status da Unidade e Papel
    E devo ver botao "+ Nova Unidade / Morador"

  Cenario: UI exibe drawer lateral de cadastro
    Quando clico em "+ Nova Unidade / Morador"
    Entao devo ver drawer lateral com campos de bloco, unidade, morador e papel

  Cenario: UI exibe wizard de importacao em lote
    Quando clico em "Importar em Lote"
    Entao devo ver modal com etapas de download, upload e preview

  Cenario: UI exibe modal de troca de titularidade
    Quando clico em "Trocar Titularidade" em uma unidade
    Entao devo ver modal com aviso de arquivamento no historico

  Cenario: UI exibe timeline de historico
    Quando clico em "Ver Historico" em uma unidade
    Entao devo ver timeline com ocupantes anteriores e datas
