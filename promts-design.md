# Prompts de Design para Telas E2E - SmartCondo

Este documento consolida:
- Quais fases/subfases devem ter tela para testes end-to-end (E2E).
- Prompts prontos para uma LLM gerar o design de cada tela.

## 1) Mapeamento de Fases com Necessidade de Tela E2E

### Fase 1 - Fundação da Infraestrutura e Núcleo Multi-tenant
- 1.1.1: Nao obrigatoria (setup estrutural).
- 1.1.2: Nao obrigatoria (bootstrap API).
- 1.1.3: Opcional tecnica (pagina de documentacao API).
- 1.2.1: Nao obrigatoria (configuracao EF/Postgres).
- 1.2.2: Nao obrigatoria (filtro global de tenant).
- 1.2.3: Nao obrigatoria (pgvector infraestrutura).
- 1.3.1: Nao obrigatoria (mensageria RabbitMQ).
- 1.3.2: Nao obrigatoria (outbox pattern).
- 1.3.3: Nao obrigatoria (Redis cache/lock/sessao).

### Fase 2 - Identity, Autenticacao e Cadastro Base
- 2.1.1: Obrigatoria (login, sessao, perfil e papeis).
- 2.1.2: Obrigatoria (troca de contexto tenant/condominio para administradores).
- 2.2.1: Obrigatoria (cadastro de administradora e condominio/tenant).
- 2.2.2: Obrigatoria (cadastro de bloco, unidade e tipo de morador).
- 2.2.3: Obrigatoria (vinculo celular/WhatsApp).

### Fase 3 - Modulo Financeiro e Cobranca
- 3.1.1: Obrigatoria (cadastro e visualizacao de faturas/boletos/itens).
- 3.1.2: Obrigatoria (simulador e explicacao de juros/multa/desconto).
- 3.1.3: Obrigatoria (integracao de pagamento e status transacional).
- 3.2.1: Obrigatoria (regua de inadimplencia e acordos).
- 3.2.2: Obrigatoria (prestacao de contas, conciliacao e relatorios).

### Fase 4 - Operacoes e Instalacoes
- 4.1.1: Obrigatoria (cadastro de areas comuns e regras).
- 4.1.2: Obrigatoria (agenda/reserva com validacao de conflito).
- 4.2.1: Obrigatoria (abertura e acompanhamento de ocorrencias).
- 4.2.2: Obrigatoria (calendario de manutencao e alertas).
- 4.2.3: Obrigatoria (assembleia virtual, votacao e ata).

### Fase 5 - Controle de Acesso e Portaria
- 5.1: Obrigatoria (cadastro e fluxo de visitantes/prestadores).
- 5.2: Obrigatoria (controle de encomendas e retiradas).

### Fase 6 - WhatsApp Engine e Ingestao Assincrona
- 6.1: Opcional operacional (painel de webhooks recebidos e monitoramento).
- 6.2: Opcional operacional (painel de fila/publicacao no broker).
- 6.3: Obrigatoria operacional (fila de processamento, reconciliacao e rastreabilidade por tenant/morador).

### Fase 7 - AI Orchestrator e Integrations
- 7.1.1: Obrigatoria (configuracao de IA por tenant, provedores e modelos).
- 7.1.2: Obrigatoria (gestao de documentos, embeddings e busca semantica).
- 7.2.1: Obrigatoria (experiencia de consulta de boletos no chat).
- 7.2.2: Obrigatoria (reserva de area comum via chat).
- 7.2.3: Obrigatoria (autorizacao de visitantes via chat).
- 7.2.4: Obrigatoria (leitura OCR de encomendas e notificacao).
- 7.2.5: Obrigatoria (triagem multimidia de ocorrencias e abertura automatica).

### Fase 8 - BDD, QA e CI/CD
- 8.1: Nao obrigatoria para usuario final (mas recomendada tela de Living Documentation).
- 8.2: Nao obrigatoria para usuario final (tecnica de pipeline).
- 8.3: Nao obrigatoria para usuario final (tecnica de CI/CD).

---

## 2) Prompt Mestre de Design (Base para todos os prompts)

Use este prompt-base antes de cada subsequente:

"Voce e um Senior Product Designer especialista em SaaS B2B para condominio e administradoras no Brasil. Gere design completo de telas web responsivas (desktop + mobile), com foco em UX operacional, acessibilidade WCAG AA, alto contraste, navegacao clara e baixa carga cognitiva. Inclua: arquitetura da informacao, layout por secoes, componentes de UI, estados vazios, loading, erro, sucesso, validacoes de formulario, microcopy em portugues-BR, feedbacks transacionais, e criterios de aceitacao visual para testes E2E. Evite placeholders genericos; use textos e dados realistas do contexto condominial."

---

## 3) Prompts por Subfase

### Fase 1.1.3 - OpenAPI/Scalar (opcional tecnico)
"Crie o design de uma tela tecnica de documentacao de API para o SmartCondo. A tela deve mostrar: lista de endpoints por modulo, status de disponibilidade, exemplo de request/response JSON, autenticacao via JWT, e bloco de teste rapido de endpoint. Inclua navegacao lateral por categorias e topo com informacoes do ambiente (dev/hml/prd). Defina criterios visuais para E2E: endpoint aparece na lista, expansao mostra payload, botao copiar funciona, filtro por modulo funciona."

### Fase 2.1.1 - Identity + JWT
"Crie o design completo do fluxo de autenticacao do SmartCondo: login por email/senha, recuperacao de senha, selecao de perfil (Sindico, Administradora, Morador, Porteiro), estado de sessao ativa e expiracao de token. Inclua tela de acesso negado e aviso de permissao insuficiente. Defina mensagens de erro para credenciais invalidas, usuario bloqueado e tenant inativo. Entregue layout desktop/mobile e checklist visual para E2E."

### Fase 2.1.2 - Middleware de Contexto Tenant
"Crie uma tela de troca de contexto para usuarios com multiplos condominios/tenants. Inclua seletor de tenant e condominio no cabecalho global, modal de confirmacao ao trocar contexto, e indicador persistente do tenant ativo. Mostre estados quando o usuario possui 1 tenant, varios tenants e nenhum tenant valido. Defina regras visuais e de UX para prevenir operacao no tenant errado e criterios de teste E2E."

### Fase 2.2.1 - Cadastro de Administradora e Condominio
"Desenhe o modulo de onboarding de administradora e condominio (criacao de tenant). Inclua wizard em etapas: dados da administradora, dados do condominio, endereco, contatos, configuracoes iniciais e revisao final. Cada etapa deve ter validacao inline e salvamento de rascunho. Inclua estados de sucesso, conflito de CNPJ e rollback de criacao. Gere especificacao visual testavel por E2E."

### Fase 2.2.2 - Blocos, Unidades e Moradores
"Crie as telas de cadastro e gestao de blocos, unidades e vinculacao de moradores com papel (proprietario ou inquilino). Inclua tabela com filtros, formulario lateral, importacao em lote por planilha e historico de alteracoes. Mostre fluxo de troca de titularidade e encerramento de vinculacao antiga sem perda de historico. Forneca criterios de interface para testes E2E."

### Fase 2.2.3 - Cadastro de Celular para WhatsApp
"Crie o design do fluxo de cadastro e verificacao de celular para autenticacao via WhatsApp. Inclua mascara de telefone BR, envio de codigo, tela de confirmacao e status do vinculo (pendente, validado, expirado). Inclua cenarios de numero ja vinculado, codigo invalido e reenvio limitado por tempo. Defina os checkpoints visuais para E2E."

### Fase 3.1.1 - Fatura, Boleto e Item de Cobranca
"Crie telas para gestao financeira com lista de faturas, detalhe da fatura, itens de cobranca e status do boleto/PIX. Inclua busca por unidade, filtros por vencimento e inadimplencia, e acao de segunda via. Na tela de detalhe, separar claramente principal, multa, juros e descontos. Defina elementos obrigatorios para validacao E2E."

### Fase 3.1.2 - Calculo de Multa/Juros/Desconto
"Crie uma tela de simulacao financeira para calcular multa, juros pro-rata e desconto ate o vencimento. Inclua campos de entrada, explicacao da formula em linguagem simples, comparativo antes/depois e trilha de auditoria do calculo. Mostre mensagens para dados invalidos e limite de arredondamento. Inclua criterios visuais para testes E2E."

### Fase 3.1.3 - Integracao Gateway de Pagamento/PIX
"Desenhe uma central de pagamentos integrada com gateway (Asaas/Juno/Ebanx/Conta Simples), mostrando status da transacao, QR Code PIX, copia e cola, webhook de confirmacao e tentativas de conciliacao. Inclua linha do tempo de eventos e acao manual de reprocessar quando permitido. Gere checklist para E2E cobrindo sucesso, pendente e falha."

### Fase 3.2.1 - Regua de Inadimplencia e Acordos
"Crie as telas da regua de inadimplencia com segmentacao de devedores, etapas de cobranca, templates de comunicacao e painel de acordos. Inclua simulador de parcelamento e assinatura de acordo. Mostre status por etapa e risco financeiro. Defina criterios para testes E2E de avancar etapa, registrar contato e fechar acordo."

### Fase 3.2.2 - Prestacao de Contas e Conciliacao
"Crie um modulo visual de prestacao de contas com pasta digital mensal, extratos, conciliacao bancaria e relatorios consolidados multicondominio. Inclua filtros por periodo e condominio, cards de inconsistencias, e exportacao PDF/Excel. Mostrar trilha de aprovacao e versoes do relatorio. Defina pontos verificaveis por E2E."

### Fase 4.1.1 - Cadastro de Areas Comuns
"Desenhe a tela de cadastro de areas comuns (salao, churrasqueira etc.) com regras de capacidade, custo por uso, janela de reserva, antecedencia minima e bloqueios de manutencao. Incluir galeria de imagens da area e politicas de uso. Defina componentes e validacoes para E2E."

### Fase 4.1.2 - Reserva com Conflito e Lock
"Crie a experiencia de agenda de reservas com calendario mensal/semanal, selecao de horario e feedback imediato de conflito. Quando houver concorrencia, exibir mensagem clara de indisponibilidade e alternativas de horario. Inclua estados de lock temporario durante confirmacao. Defina criterios E2E para concorrencia simulada."

### Fase 4.2.1 - Ocorrencias com Upload
"Crie telas de abertura e acompanhamento de ocorrencias com upload de fotos, classificacao de prioridade, responsavel e SLA. Inclua linha do tempo do chamado, comentarios, anexos e alteracao de status (aberto, em andamento, resolvido). Defina requisitos visuais para E2E incluindo anexos e transicoes de status."

### Fase 4.2.2 - Plano de Manutencao
"Desenhe um calendario de manutencao preventiva para ativos prediais (elevador, bomba, para-raios), com alertas, recorrencia e comprovantes de execucao. Incluir visao calendario e visao lista, com semaforo de risco por atraso. Defina checkpoints de E2E para criar plano, receber alerta e concluir manutencao."

### Fase 4.2.3 - Assembleia Virtual
"Crie o design do modulo de assembleia virtual com pautas, votacao, quoruns, cronometro de sessao e geracao de ata. Incluir experiencia para morador votar, revisar voto e confirmar participacao. Para administracao, mostrar apuracao em tempo real e trilha de auditoria. Defina criterios E2E para abertura, voto e encerramento."

### Fase 5.1 - Visitantes e Prestadores
"Crie telas de portaria para cadastro e controle de visitantes/prestadores: pre-autorizacao, check-in, check-out, validade da liberacao e comprovante visual para entrada. Incluir busca rapida por nome/documento/placa e indicadores de restricao. Defina fluxos E2E ponta a ponta para pre-autorizacao e entrada efetiva."

### Fase 5.2 - Encomendas
"Desenhe o fluxo de encomendas com registro na portaria, foto do pacote, notificacao ao morador e baixa de retirada com assinatura digital. Incluir status (recebida, notificada, retirada, devolvida), filtro por unidade e SLA de retirada. Defina criterios E2E para registro, notificacao e entrega final."

### Fase 6.1 - Webhook Inbound (opcional operacional)
"Crie um painel operacional de webhooks WhatsApp recebidos com tabela em tempo real, filtros por tenant, status de validacao e detalhe do payload. Incluir metricas de volume/minuto e latencia de resposta HTTP 200. Definir interface para troubleshooting e criterios E2E de rastreabilidade."

### Fase 6.2 - Publicacao RabbitMQ (opcional operacional)
"Desenhe uma tela de monitoramento de publicacao no broker com status por mensagem (publicada, reprocessada, erro), tentativas e dead-letter. Incluir grafico de throughput e alertas por fila. Defina criterios visuais para testes E2E de monitoramento operacional."

### Fase 6.3 - Consumidor WhatsAppInboundConsumer
"Crie um painel de processamento assincrono do consumidor WhatsAppInboundConsumer mostrando correlacao entre mensagem inbound, tenant resolvido, morador identificado e resultado final da triagem. Inclua fila de pendencias para mensagens nao resolvidas e acao manual de reconciliar. Defina checks E2E para rastrear uma mensagem do webhook ao resultado final."

### Fase 7.1.1 - Setup Semantic Kernel
"Crie telas de configuracao de IA por tenant com selecao de provedor (OpenAI/Azure OpenAI), modelo, limites de custo e politicas de seguranca. Incluir teste de conexao, historico de uso e alertas de cota. Defina criterios E2E para salvar configuracao, validar credenciais e alternar modelo ativo."

### Fase 7.1.2 - Pipeline RAG com Pgvector
"Desenhe um modulo de conhecimento com upload de documentos (convencao/regimento), status de ingestao, chunking, embeddings e busca semantica. Incluir visualizacao de trechos recuperados com score de similaridade e origem do documento. Defina criterios E2E para upload, processamento e consulta com evidencias."

### Fase 7.2.1 - Plugin GetPendingBoletos no Chat
"Crie a experiencia de chat para consulta de boletos pendentes. A interface deve mostrar resposta estruturada com valor, vencimento, chave PIX copia e cola e link de PDF. Incluir CTA para copiar PIX e registrar evento de clique. Defina criterios E2E para consulta, retorno e acao do usuario."

### Fase 7.2.2 - Plugin ReserveCommonArea no Chat
"Desenhe a UX conversacional para reserva de area comum via chat, incluindo desambiguacao da area, selecao de data/horario, validacao de disponibilidade e confirmacao final. Exibir alternativa quando horario estiver ocupado. Defina checkpoints E2E para fluxo feliz e fluxo de conflito."

### Fase 7.2.3 - Plugin AuthorizeGuest no Chat
"Crie a interface de chat para autorizacao de visitante com coleta de nome, documento e data da visita. Mostrar resumo antes da confirmacao e retorno com protocolo de liberacao. Definir tratamento para dados incompletos e criterios E2E de confirmacao e consulta de protocolo."

### Fase 7.2.4 - OCR de Etiquetas + IA
"Crie o design do fluxo de leitura inteligente de etiquetas de encomenda: upload/captura de imagem, extracao OCR, revisao de campos e envio de notificacao ao morador. Mostrar confianca da extracao e necessidade de correcao manual quando baixa confianca. Defina criterios E2E para extrair, corrigir e notificar."

### Fase 7.2.5 - Triagem de Ocorrencias por Foto/Audio
"Crie uma experiencia de IA para triagem de ocorrencias recebidas por foto/audio, com classificacao automatica, prioridade sugerida, categoria e abertura de chamado pre-preenchido. Exibir explicacao resumida da decisao da IA e permitir revisao humana antes de confirmar. Defina validacoes E2E para transparencia e controle humano."

### Fase 8.1 - Living Documentation (recomendado tecnico)
"Crie o design de um portal interno de Living Documentation com cenarios BDD em portugues, status de execucao, historico por build e filtros por fase/subfase. Incluir visualizacao de falhas com evidencias e links para casos E2E. Defina criterios E2E para navegacao, filtro e detalhamento de cenario."

---

## 4) Prompt de Padronizacao Visual Global

"Com base nas telas ja desenhadas do SmartCondo, gere um Design System consistente contendo: paleta de cores acessivel, tipografia, espacamento, grid, componentes reutilizaveis (botao, input, select, tabela, card, modal, toast, badge, timeline, calendar, chat bubble), estados de interacao e regras de responsividade. Inclua naming de componentes para handoff com time de frontend e checklist de consistencia para automacao E2E."

## 5) Prompt de Criticidade E2E (QA UX)

"Revise todos os fluxos desenhados e gere uma matriz de criticidade E2E por tela: fluxo critico, fluxo alternativo, estados de erro, dependencia externa, impacto no negocio e prioridade de automacao (P0/P1/P2). Entregar em formato tabular com recomendacoes de ordem de implementacao dos testes."
