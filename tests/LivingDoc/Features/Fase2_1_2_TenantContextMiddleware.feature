# language: pt-BR
Funcionalidade: Middleware de injecao de contexto tenant e troca de contexto
  Como operador do SmartCondo
  Eu quero que o tenant ativo seja resolvido automaticamente por JWT ou header de webhook
  Para garantir isolamento multi-tenant seguro em toda a API

  Contexto:
    Dado que o middleware de contexto tenant esta registrado apos autenticacao
    E existe um usuario autenticado com membership no tenant "1" condominio "10"

  Cenario: JWT com claim TenantId injeta contexto na requisicao autenticada
    Dado que possuo JWT contextual com claim "TenantId" igual a "1"
    Quando eu faco GET para "/api/auth/context" com o token
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter "isSuccess" com valor "true"
    E a resposta deve conter tenantId "1" e isResolved "true"

  Cenario: Requisicao sem JWT e sem header mantem contexto nao resolvido
    Quando eu faco GET para "/api/auth/context" sem autenticacao
    Entao o status HTTP deve ser 401 Unauthorized

  Cenario: Header X-Tenant-Id em rota de webhook injeta contexto
    Quando eu faco GET para "/api/webhooks/context-probe" com header "X-Tenant-Id" "2"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter tenantId "2" e isResolved "true"

  Cenario: Header X-Tenant-Id em rota comum nao sobrescreve JWT
    Dado que possuo JWT contextual com claim "TenantId" igual a "1"
    Quando eu faco GET para "/api/auth/context" com o token e header "X-Tenant-Id" "99"
    Entao a resposta deve conter tenantId "1"

  Cenario: Troca de perfil atualiza contexto do proximo request
    Dado que possuo tokens validos de login com multiplos perfis
    Quando eu seleciono perfil do tenant "2"
    E faco GET para "/api/auth/context" com o novo token
    Entao a resposta deve conter tenantId "2"

  Cenario: Listagem de perfis disponiveis para troca de contexto
    Dado que possuo JWT contextual valido
    Quando eu faco GET para "/api/auth/profiles"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter lista de perfis com displayLabel e role

  Cenario: UI multi-tenant exibe seletor com dropdown ativo
    Dado que o usuario possui mais de um perfil ativo
    Quando acessa o dashboard autenticado
    Entao deve ver o seletor de contexto no cabecalho com busca e badges de role

  Cenario: UI single-tenant exibe badge estatico com cadeado
    Dado que o usuario possui apenas um perfil ativo
    Quando acessa o dashboard autenticado
    Entao deve ver badge estatico do condominio sem dropdown

  Cenario: UI sem tenant ativo exibe estado vazio com CTA
    Dado que o usuario nao possui contexto ativo
    Quando acessa o dashboard
    Entao deve ver banner "Sem tenant ativo" e botao "Solicitar Acesso ou Vincular Condominio"

  Cenario: Troca de contexto exige confirmacao modal Stitch
    Dado que o usuario seleciona outro condominio no dropdown
    Quando confirma a troca no modal
    Entao o contexto ativo deve ser atualizado e a pagina recarregada
