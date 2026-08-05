# language: pt-BR
Funcionalidade: Gestao de Faturas, Boletos e Itens de Cobranca (Modulo Financeiro)
  Como sindico ou administradora do condominio
  Eu quero emitir, consultar e gerenciar faturas com boletos e itens detalhados
  Para controlar a arrecadacao condominial e permitir o pagamento pelos moradores

  Contexto:
    Dado que o modulo financeiro esta ativo na API
    E que estou autenticado no condomínio "1" com tenantId "1"

  Cenario: Emissao de fatura com itens e boleto associado retorna 201 Created
    Quando eu envio POST para "/api/financial/invoices" com moradorId "10", competencia "2026-08" e itens de cobrança
    Entao o status HTTP deve ser 201 Created
    E a resposta deve conter "isSuccess" com valor "true"
    E o valor total da fatura deve ser a soma dos itens de cobrança
    E o boleto associado deve conter linha digitável e código PIX

  Cenario: Consulta de faturas com filtros por competencia e status
    Dado que existem faturas cadastradas na competencia "2026-08"
    Quando eu consulto GET "/api/financial/invoices" com filtro competencia "2026-08"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter a lista paginada de faturas da competencia filtrada

  Cenario: Consulta detalhada de fatura por ID retorna itens e breakdown financeiro
    Dado que existe uma fatura cadastrada com ID valido
    Quando eu consulto GET "/api/financial/invoices/{id}"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve detalhar os itens de cobrança, valor principal e dados do boleto

  Cenario: Cancelamento de fatura pendente altera status para Cancelado
    Dado que existe uma fatura pendente com ID valido
    Quando eu envio POST para "/api/financial/invoices/{id}/cancel"
    Entao o status HTTP deve ser 200 OK
    E o status da fatura deve mudar para "Cancelado"

  Cenario: Tentar consultar fatura de outro tenant deve retornar vindo do Global Query Filter
    Dado que estou autenticado com tenantId "1"
    E existe uma fatura pertencente ao tenantId "2"
    Quando eu consulto GET "/api/financial/invoices/{id_do_tenant_2}"
    Entao o status HTTP deve ser 404 Not Found
