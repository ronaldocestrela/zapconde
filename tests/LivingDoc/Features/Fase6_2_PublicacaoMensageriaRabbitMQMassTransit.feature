# language: pt-BR
Funcionalidade: Publicação Assíncrona de Mensagens do WhatsApp no RabbitMQ via MassTransit e Outbox Pattern

  Como o sistema Smart Condo SaaS
  Quero publicar os eventos de mensagens recebidas do WhatsApp via Evolution API imediatamente em uma fila do RabbitMQ utilizando MassTransit e Transactional Outbox Pattern
  Para responder com HTTP 200 OK instantâneo para a Evolution API e garantir o processamento assíncrono seguro com isolamento multi-tenant

  Cenario: Publicação com sucesso de evento de mensagem recebida no barramento MassTransit
    Dado que a instância "condo-central" está ativa no condomínio "1"
    Quando o webhook da Evolution API recebe uma mensagem de texto "Gostaria de agendar o salão de festas" do número "+5575999999999"
    Então o webhook deve retornar uma resposta HTTP 200 OK instantânea com Result de sucesso
    E o evento de integração "WhatsAppMessageReceivedEvent" deve ser publicado via MassTransit
    E o log de webhook em "whatsapp.WebhookLogs" deve estar gravado com status "Received"

  Cenario: Garantia de consistência transacional via Outbox Pattern no PostgreSQL
    Dado que a mensagem "BAE5F9998877" é recebida no endpoint de webhook
    Quando o serviço persiste o log "WhatsAppWebhookLog" no banco PostgreSQL
    Então a mensagem "WhatsAppMessageReceivedEvent" deve ser gravada atomicamente na tabela de Outbox do "WhatsAppDbContext" na mesma transação
    E o envio para o broker RabbitMQ deve ser garantido mesmo sob oscilações da rede

  Cenario: Resposta instantânea e não-bloqueante ao gateway da Evolution API
    Dado uma requisição HTTP POST para o endpoint "/api/whatsapp/webhook/evolution"
    Quando o payload válido é processado
    Então o tempo de processamento HTTP deve ser inferior a 100ms
    E a execução de downstream não deve bloquear o ciclo da requisição HTTP

  Cenario: Preservação de atributos de isolamento e metadados no evento enfileirado
    Dado o recebimento de um payload de imagem com legenda "Comprovante pix" do remetente "+5575988887777" na instância "condo-central"
    Quando o evento "WhatsAppMessageReceivedEvent" é instanciado para publicação
    Então o evento deve conter os campos "TenantId", "CondoId", "MessageId", "SenderPhone", "MessageType", "MessageText" e "RawPayloadJson" preenchidos corretamente
