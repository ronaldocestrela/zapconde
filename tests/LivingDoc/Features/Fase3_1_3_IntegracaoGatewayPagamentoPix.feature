# language: pt-BR
Funcionalidade: Integracao com Gateway de Pagamento e PIX para Faturas Condominiais
  Como morador, sindico ou administradora do condominio
  Eu quero gerar cobrancas de boleto bancario e PIX via gateway de pagamento
  Para permitir o pagamento facil e a conciliacao automatica via webhooks idempotentes

  Contexto:
    Dado que o modulo financeiro e a integracao de gateway estao ativos no sistema
    E o provedor de pagamento configurado e "Asaas" ou "Mock"

  Cenario: Gerar cobranca hibrida de boleto e PIX para uma fatura existente
    Dado que existe uma fatura cadastrada com valor original de "350.00" e vencimento em "2026-08-20"
    Quando eu solicito a geração de pagamento no gateway via POST "/api/financial/invoices/{id}/generate-payment"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter "isSuccess" igual a "true"
    E os dados de pagamento devem conter linha digitavel, codigo de barras e chave PIX Copia e Cola
    E a imagem do QR Code visual em Base64 deve ser retornada
    E o boleto vinculado deve registrar o ID da cobrança externa no gateway

  Cenario: Consultar informacoes de pagamento de uma fatura com boleto gerado
    Dado que uma fatura possui um boleto gerado no gateway com PIX e linha digitavel
    Quando eu solicito a consulta de pagamento via GET "/api/financial/invoices/{id}/payment-info"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve retornar os dados completos do PIX, QR Code, linha digitavel e PDF URL

  Cenario: Processar Webhook de pagamento recebido com garantia de idempotencia
    Dado que existe um boleto pendente com ID externo de cobrança "pay_asaas_12345"
    Quando o endpoint de Webhook recebe uma notificacao "PAYMENT_RECEIVED" com o token de acesso valido
    Entao o status HTTP deve ser 200 OK
    E a fatura vinculada deve ter o status atualizado para "Pago"
    E o boleto vinculado deve ter o status atualizado para "Pago"
    Quando o mesmo Webhook "PAYMENT_RECEIVED" e reenviado com a mesma chave idempotente
    Entao o sistema deve ignorar o duplo processamento e retornar sucesso de forma idempotente

  Cenario: Recusar notificação de Webhook com token de acesso invalido
    Quando o endpoint de Webhook recebe um payload sem o header "X-Asaas-Access-Token" correto
    Entao o status HTTP deve ser 401 Unauthorized ou 400 Bad Request
    E o processamento da cobranca nao deve ser executado

  Cenario: Sincronizar status de pagamento sob demanda com a API do gateway
    Dado que existe uma fatura pendente com cobrança vinculada ao gateway
    Quando eu solicito a sincronizacao manual via POST "/api/financial/invoices/{id}/sync-payment"
    Entao o status HTTP deve ser 200 OK
    E o sistema deve consultar a API do gateway e atualizar os dados do boleto localmente
