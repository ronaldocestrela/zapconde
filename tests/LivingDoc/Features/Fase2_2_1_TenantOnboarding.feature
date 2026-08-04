# language: pt-BR
Funcionalidade: Onboarding de Administradora e Condominio (Criacao de Tenant)
  Como operador de uma administradora
  Eu quero cadastrar administradora e condominio via wizard
  Para iniciar a gestao no SmartCondo com isolamento multi-tenant

  Contexto:
    Dado que o modulo de onboarding de tenant esta disponivel na API

  Cenario: Criacao feliz de tenant retorna 201 com dados do tenant
    Quando eu envio POST para "/api/tenants/onboarding" com payload valido de onboarding
    Entao o status HTTP deve ser 201 Created
    E a resposta deve conter "isSuccess" com valor "true"
    E a resposta deve conter tenantId e condoId

  Cenario: Conflito de CNPJ retorna 409
    Dado que existe administradora com CNPJ "07.526.557/0001-00"
    Quando eu consulto GET "/api/tenants/cnpj/07526557000100/status"
    Entao o status HTTP deve ser 409 Conflict
    E a resposta deve conter mensagem de CNPJ ja cadastrado

  Cenario: Validacao de CNPJ invalido retorna 422
    Quando eu consulto GET "/api/tenants/cnpj/00000000000000/status"
    Entao o status HTTP deve ser 422 Unprocessable Entity

  Cenario: Validacao de dia de vencimento invalido retorna 422
    Quando eu envio POST para "/api/tenants/onboarding" com dia de vencimento "32"
    Entao o status HTTP deve ser 422 Unprocessable Entity

  Cenario: Falha simulada na transacao retorna rollback sem persistencia
    Quando eu envio POST para "/api/tenants/onboarding" com flag simulateRollback
    Entao o status HTTP deve ser 500 Internal Server Error
    E a resposta deve conter mensagem de rollback efetuado

  Cenario: Salvar e recuperar rascunho do wizard
    Quando eu envio POST para "/api/tenants/onboarding/draft" com dados parciais
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter draftId
    Quando eu consulto GET do rascunho salvo
    Entao a resposta deve conter os dados parciais do wizard

  Cenario: UI wizard exibe stepper com 6 etapas
    Quando acesso a pagina "/onboarding"
    Entao devo ver stepper com etapas de administradora, condominio, endereco, contatos, configuracoes e revisao

  Cenario: UI exibe badge de rascunho salvo automaticamente
    Dado que o rascunho foi salvo automaticamente
    Quando estou no wizard de onboarding
    Entao devo ver badge "Rascunho salvo automaticamente"

  Cenario: UI exibe alerta de conflito de CNPJ na etapa 1
    Dado que informo CNPJ ja cadastrado
    Quando avanco na etapa de administradora
    Entao devo ver callout de conflito com botao "Solicitar Suporte"

  Cenario: UI exibe overlay de sucesso apos criacao
    Dado que a criacao do tenant foi concluida com sucesso
    Quando o wizard finaliza
    Entao devo ver titulo "Tenant Criado com Sucesso!"
    E botao "Acessar Painel do Condominio"

  Cenario: UI exibe banner de rollback em falha de criacao
    Dado que ocorreu falha na transacao de criacao
    Quando o wizard tenta finalizar
    Entao devo ver banner de rollback com acao "Tentar Novamente"
