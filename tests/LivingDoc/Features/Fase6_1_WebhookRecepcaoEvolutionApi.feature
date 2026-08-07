# language: pt-BR
Funcionalidade: Recepção e Ingestão de Webhooks da Evolution API do WhatsApp

  Como administrador do sistema ou síndico do condomínio
  Quero que o sistema receba webhooks de mensagens do WhatsApp via Evolution API de forma assíncrona, idempotente e isolada por tenant
  Para permitir o atendimento automatizado e registro auditável de mensagens dos moradores

  Cenario: Recepção com sucesso de mensagem de texto via Evolution API
    Dado que a instância do WhatsApp "condo-central" está cadastrada e ativa para o condomínio "1"
    Quando o webhook da Evolution API recebe o evento "messages.upsert" com a mensagem "Olá, preciso da 2 via do boleto" enviada por "+5575999999999"
    Então o webhook deve responder com HTTP 200 OK e Result indicando sucesso
    E um registro de log deve ser salvo em "whatsapp.WebhookLogs" com o status "Received"
    E o remetente deve ser identificado como "+5575999999999" e o tipo de mensagem como "Text"

  Cenario: Prevenção de duplicidade por idempotência na recepção do webhook
    Dado que uma mensagem com o MessageID "BAE5F123456789" já foi recebida e registrada para a instância "condo-central"
    Quando o webhook recebe novamente um payload idêntico com o MessageID "BAE5F123456789"
    Então o webhook deve responder com HTTP 200 OK e Result indicando duplicidade ignorada
    E não deve criar um segundo registro duplicado no banco de dados

  Cenario: Recepção de mensagem de mídia (imagem ou documento) via Evolution API
    Dado que a instância do WhatsApp "condo-central" está ativa para o condomínio "1"
    Quando o webhook recebe o evento "messages.upsert" contendo uma imagem com legenda "Comprovante de pagamento" enviada por "+5575888888888"
    Então o status do log deve ser "Received"
    E o tipo de mensagem deve ser registrado como "Image"
    E a legenda "Comprovante de pagamento" deve ser armazenada no log

  Cenario: Tentativa de envio de webhook para instância inexistente ou token inválido
    Dado que o webhook recebe um payload contendo uma chave de API ou instância não cadastrada "instancia-desconhecida"
    Quando a verificação de segurança do webhook é executada
    Então a requisição deve ser rejeitada ou registrada com falha indicando erro de autenticação/instância
