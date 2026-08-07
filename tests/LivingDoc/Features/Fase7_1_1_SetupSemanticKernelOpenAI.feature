# language: pt-BR
Funcionalidade: Setup do Microsoft.SemanticKernel integrado ao OpenAI e Azure OpenAI

  Como o sistema Smart Condo SaaS
  Quero configurar e orquestrar instâncias do Microsoft.SemanticKernel por condomínio (Tenant)
  Para permitir execuções de prompts LLM com isolamento multi-tenant, auditoria de consumo e métricas de desempenho

  Cenario: Configuração do Semantic Kernel com provedor OpenAI por Tenant
    Dado que o condomínio "1" deseja utilizar o provedor "OpenAI" com o modelo "gpt-4o-mini"
    E fornece a chave API "sk-test-key-12345" e temperatura "0.7"
    Quando o endpoint "POST /api/ai/config" for chamado com os dados de configuração
    Então a configuração do kernel deve ser salva no PostgreSQL no schema "ai" vinculada ao TenantId "1"
    E a chave API deve ser armazenada com segurança e mascarada nos retornos de consulta

  Cenario: Execução de prompt via Semantic Kernel e registro de auditoria
    Dado que o condomínio "1" possui a configuração do Semantic Kernel ativa
    Quando um prompt "Qual o horário de funcionamento do salão de festas?" for enviado para "POST /api/ai/prompt/execute"
    Então o Semantic Kernel deve processar o prompt e retornar a resposta gerada
    E um registro de auditoria deve ser persistido na tabela "ai.ExecutionLogs" com TenantId "1"
    E a métrica deve conter a contagem de tokens (prompt, completion e total) e o tempo de execução em milissegundos

  Cenario: Execução em modo MockLocal para ambientes de desenvolvimento e teste
    Dado que o condomínio "1" está configurado com o provedor "MockLocal"
    Quando um prompt "Olá IA" for executado no playground de IA
    Então o serviço deve responder utilizando a simulação local sem realizar chamadas externas pagas
    E a resposta deve retornar status de sucesso com result encapsulado no tipo "Result<ExecutePromptResponseDto>"

  Cenario: Tentativa de execução sem configuração ativa no Tenant
    Dado que o condomínio "2" não possui nenhuma configuração de IA cadastrada
    Quando o endpoint "POST /api/ai/prompt/execute" for invocado no TenantId "2"
    Então o sistema deve retornar um erro de negócio tratável com código de falha e mensagem explicativa via Result pattern
