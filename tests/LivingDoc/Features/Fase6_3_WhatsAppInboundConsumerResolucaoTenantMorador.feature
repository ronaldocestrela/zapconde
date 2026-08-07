# language: pt-BR
Funcionalidade: Consumo em Background e Resolução de Tenant e Morador no WhatsApp Webhook

  Como o sistema Smart Condo SaaS
  Quero que o consumidor em background "WhatsAppInboundConsumer" processe os eventos de mensagens do WhatsApp
  Para extrair o telefone, identificar o Tenant e o Morador no Redis com fallback no PostgreSQL, e garantir a idempotência

  Cenario: Resolução de morador cadastrado via cache Redis
    Dado que a mensagem "BAE5F9990001" é enfileirada para o remetente "+5575999999999" na instância "condo-central"
    E o mapeamento do morador "+5575999999999" já está em cache no Redis com TenantId "1" e MoradorId "42"
    Quando o "WhatsAppInboundConsumer" consome o evento "WhatsAppMessageReceivedEvent"
    Então o "WhatsAppWebhookLog" deve ser atualizado com status "Processed" e MoradorId "42"
    E a resolução deve indicar "CacheHit" no Redis sem consultar o banco de dados
    E o evento de integração "WhatsAppMessageProcessedEvent" deve ser publicado downstream

  Cenario: Resolução via fallback PostgreSQL em caso de cache miss com povoamento do Redis
    Dado que a mensagem "BAE5F9990002" é enfileirada para o remetente "+5575988887777" na instância "condo-central"
    E o número "+5575988887777" não está armazenado no cache Redis
    E existe um morador cadastrado no PostgreSQL com o telefone E.164 "+5575988887777", TenantId "1" e MoradorId "88"
    Quando o "WhatsAppInboundConsumer" consome o evento "WhatsAppMessageReceivedEvent"
    Então o serviço deve buscar o morador no PostgreSQL via "IResidentLookupService"
    E deve atualizar o cache Redis com a chave "wpp:morador:phone:+5575988887777"
    E o "WhatsAppWebhookLog" deve ser atualizado com status "Processed" e MoradorId "88"

  Cenario: Processamento de mensagem de remetente não cadastrado como morador
    Dado que a mensagem "BAE5F9990003" é enfileirada para o remetente "+5575977776666" na instância "condo-central"
    E o telefone "+5575977776666" não pertence a nenhum morador no PostgreSQL nem no Redis
    Quando o "WhatsAppInboundConsumer" consome o evento "WhatsAppMessageReceivedEvent"
    Então o "WhatsAppWebhookLog" deve ser atualizado com status "Processed" e MoradorId nulo
    E a mensagem deve ser marcada para triagem / atendimento de visitante ou não identificado

  Cenario: Trava distribuída Redis evitando processamento concorrente duplicado
    Dado duas execuções simultâneas do "WhatsAppInboundConsumer" para o mesmo MessageId "BAE5F9990004"
    Quando a primeira thread adquire a trava distribuída "wpp:lock:msg:BAE5F9990004" no Redis
    Então a segunda thread deve aguardar a trava ou ignorar a duplicidade sem corromper o estado do banco
