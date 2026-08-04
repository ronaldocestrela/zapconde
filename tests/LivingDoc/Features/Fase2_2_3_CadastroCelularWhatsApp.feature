# language: pt-BR
Funcionalidade: Cadastro e verificacao do celular do morador via WhatsApp
  Como administrador ou sindico
  Eu quero vincular e validar o celular de um morador
  Para habilitar autenticacao e notificacoes seguras pelo WhatsApp

  Contexto:
    Dado que estou autenticado com tenantId "1" e condoId "10"
    E que existe um morador no contexto ativo

  Cenario: Solicitacao de codigo registra status pendente
    Quando envio POST para "/api/residents/{moradorId}/phone/request-code" com telefone brasileiro valido
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter status "AguardandoValidacao"

  Cenario: Telefone brasileiro invalido retorna erro processavel
    Quando solicito codigo para o telefone "119999"
    Entao o status HTTP deve ser 422 Unprocessable Entity

  Cenario: Numero validado por outro morador retorna conflito
    Dado que o telefone informado ja esta validado por outro morador
    Quando solicito um codigo para esse telefone
    Entao o status HTTP deve ser 409 Conflict

  Cenario: Codigo invalido nao confirma o telefone
    Dado que solicitei um codigo de verificacao
    Quando envio POST para "/api/residents/{moradorId}/phone/verify" com codigo incorreto
    Entao o status HTTP deve ser 422 Unprocessable Entity
    E o status do telefone deve continuar "AguardandoValidacao"

  Cenario: Codigo expirado atualiza o status
    Dado que o codigo de verificacao expirou
    Quando tento confirmar o telefone
    Entao o status HTTP deve ser 422 Unprocessable Entity
    E o status do telefone deve ser "Expirado"

  Cenario: Codigo correto valida e vincula o telefone
    Dado que solicitei um codigo de verificacao
    Quando envio o codigo correto
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter status "Validado"
    E deve informar a data de validacao

  Cenario: Reenvio respeita intervalo minimo
    Dado que solicitei um codigo ha menos de 60 segundos
    Quando solicito o reenvio
    Entao o status HTTP deve ser 409 Conflict
    E a resposta deve informar o tempo restante

  Cenario: Limite de envios bloqueia novas tentativas
    Dado que solicitei cinco codigos nos ultimos 15 minutos
    Quando solicito outro reenvio
    Entao o status HTTP deve ser 409 Conflict
    E a resposta deve informar para aguardar 15 minutos

  Cenario: Isolamento impede acesso a morador de outro tenant
    Dado que o morador pertence ao tenant "2"
    Quando consulto GET "/api/residents/{moradorId}/phone/status" no tenant "1"
    Entao o status HTTP deve ser 404 Not Found

  Cenario: UI apresenta cadastro com mascara brasileira
    Quando acesso "/verificar-celular?moradorId={moradorId}"
    Entao devo ver o prefixo "+55"
    E o campo deve usar a mascara "(00) 00000-0000"
    E devo ver o botao "Enviar Codigo via WhatsApp"

  Cenario: UI apresenta verificacao OTP responsiva
    Dado que o codigo foi enviado
    Entao devo ver seis campos individuais para o codigo
    E devo ver o contador para reenvio
    E devo ver os estados "Aguardando Validacao", "Numero Validado & Vinculado" e "Codigo Expirado"
