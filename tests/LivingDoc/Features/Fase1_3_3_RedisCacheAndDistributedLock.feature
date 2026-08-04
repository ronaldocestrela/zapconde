# language: pt-BR
Funcionalidade: Configuracao do Cliente Redis para Cache, Distributed Lock e Sessao de Chat

  Como desenvolvedor da plataforma SmartCondo
  Quero integrar o cliente Redis na infraestrutura da solucao
  Para prover cache com isolamento por tenant, locks distribuidos e gerenciamento de sessao de chat

  Contexto:
    Dado que a solucao possui infraestrutura em .NET 10
    E a subfase 1.3.3 de integracao com Redis esta em implementacao

  Cenario: Dependencias de pacotes Redis e Testcontainers estao presentes
    Quando eu inspecionar os projetos da solucao
    Entao a infraestrutura deve referenciar o pacote "StackExchange.Redis"
    E a suite de testes de integracao deve referenciar o pacote "Testcontainers.Redis"

  Cenario: Operacoes de cache aplicam isolamento automatico por tenant
    Dado que o ICacheService e injetado com contexto de tenant "10"
    Quando uma chave de cache "taxa_condominial" for salva
    Entao o registro no Redis deve ser prefixado no formato "tenant:10:taxa_condominial"
    E a leitura da chave pelo mesmo tenant deve retornar o objeto serializado

  Cenario: Distributed Lock previne concorrencia simultanea
    Dado que dois processos tentam adquirir o lock distribuido "reserva:area:1:2026-08-04"
    Quando o primeiro processo adquire o lock com sucesso
    Entao a tentativa do segundo processo deve falhar ou aguardar ate o timeout
    E ao liberar o lock o recurso fica disponivel novamente

  Cenario: Sessao de chat e mantida com TTL configuravel
    Dado que o IChatSessionService registra a sessao conversacional do morador no condominio
    Quando o estado do chat for atualizado com expiracao de 30 minutos
    Entao as mensagens recentes devem ser recuperadas da chave de sessao
    E a sessao deve expirar apos a duracao configurada

  Cenario: Conexao com Redis e validada pelo endpoint de Health Check
    Dado que a infraestrutura registra o RedisHealthCheck
    Quando a API receber uma requisicao HTTP GET em "/health/ready"
    Entao a resposta de prontidao deve incluir o status do componente "redis"
