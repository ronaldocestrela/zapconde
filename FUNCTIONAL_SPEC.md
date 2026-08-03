# 🏛️ Especificação Funcional Completa do Sistema SaaS

Este documento reúne **todas as funcionalidades** do sistema de gestão condominial inteligente em .NET 10. Ele serve de referência funcional direta para desenvolvimento via TDD, criação de cenários BDD (.feature) e mapeamento dos plugins do Semantic Kernel / Agente de IA.

---

## 🏬 1. Módulo: Identity & Multi-Tenancy

Gerencia a estrutura organizacional de administradoras, condomínios, usuários e o isolamento seguro de dados.

* **FN-ID-01 | Cadastro e Gestão de Tenants:** Suporte a múltiplas administradoras com isolamento total de dados (`tenant_id` via EF Core Global Query Filter).
* **FN-ID-02 | Autenticação & Autorização RBAC:** Emissão de JWT com suporte a papéis e permissões (Administradora, Síndico, Zelador, Portaria, Condômino).
* **FN-ID-03 | Gestão de Unidades e Moradores:** Mapeamento de blocos, apartamentos, proprietários, inquilinos e dependentes.
* **FN-ID-04 | Vínculo de WhatsApp:** Associação do número de telefone celular ao perfil do morador para validação e autenticação contextual das conversas.

---

## 💰 2. Módulo: Financial & Billing

Responsável pela gestão financeira das contas do condomínio, cobrança e automação de inadimplência.

### 💳 Funcionalidades Convencionais (Portal & Backend)
* **FN-FIN-01 | Emissão e Liquidação de Boletos / PIX:** Automação na geração da taxa condominial com cálculo progressivo de juros e multas por atraso.
* **FN-FIN-02 | Régua e Controle de Inadimplência:** Histórico consolidado de débitos, notificações automáticas pré e pós-vencimento e gestão de acordos de cobrança.
* **FN-FIN-03 | Prestação de Contas Digital:** Montagem de pastas digitais com extratos, notas fiscais e relatórios financeiros mensais.
* **FN-FIN-04 | Conciliação Bancária & Relatórios Consolidados:** Reconciliação bancária ágil para múltiplas contas e visão financeira unificada para administradoras.
* **FN-FIN-05 | Previsão Orçamentária:** Ferramentas de planejamento financeiro, comparativo de orçado vs. realizado e relatórios de fluxo de caixa.

### 🤖 Integração com IA (WhatsApp & Chatbot)
* **FN-FIN-IA01 | Emissão de 2ª Via de Boleto/PIX por Chat:** O morador solicita via WhatsApp (*"Me envia o boleto do mês"*) e a IA executa o *Function Calling* `GetPendingBoletos(moradorId)`, enviando a linha digitável, chave PIX Copia e Cola e PDF em segundos.
* **FN-FIN-IA02 | Resumo Didático da Prestação de Contas:** A IA sintetiza balancetes extensos em resumos em texto visual e direto para os moradores (ex: *"Os maiores custos deste mês foram reforma da piscina e água"*).

---

## 🏢 3. Módulo: Operations & Facilities

Gerencia o uso do espaço físico, manutenção predial e comunicação institucional do condomínio.

### 📅 Funcionalidades Convencionais (App Mobile & Web)
* **FN-OPE-01 | Gestão de Reservas de Áreas Comuns:** Agendamento de salão de festas, churrasqueiras, quadras com controle de capacidade e regras rígidas de bloqueio de horários.
* **FN-OPE-02 | Mural de Avisos e Comunicados:** Publicação de comunicados oficiais por bloco, condomínio ou unidade, com confirmação de leitura.
* **FN-OPE-03 | Gestão de Ocorrências e Chamados:** Canal para registro direto pelo morador com acompanhamento em tempo real e mudança de status pela gestão.
* **FN-OPE-04 | Plano de Manutenção Preventiva:** Calendário e controle de manutenções periódicas obrigatórias (elevadores, bombas d'água, caixa d'água, para-raios).
* **FN-OPE-05 | Assembleia Virtual:** Realização de reuniões digitais, votações online com validade jurídica, contagem de quórum e registro de atas.

### 🤖 Integração com IA (WhatsApp & Chatbot)
* **FN-OPE-IA01 | Reserva de Espaços por Linguagem Natural:** O morador solicita no chat (*"Reserve a churrasqueira sábado que vem à noite"*) e a IA executa o *Function Calling* `ReserveCommonArea(areaId, data, moradorId)` após validar colisões de horário.
* **FN-OPE-IA02 | Abertura e Triagem de Ocorrências por Mídia:** O morador envia foto/áudio (*"Vazamento na garagem"*); a IA lê/transcreve, classifica a gravidade e abre o chamado direcionado ao zelador.
* **FN-OPE-IA03 | RAG do Regimento Interno (FAQ Inteligente):** O morador faz perguntas operacionais (*"Até que horas posso fazer barulho?"*) e a IA consulta os embeddings da convenção/regimento via **pgvector** para responder com precisão.
* **FN-OPE-IA04 | Resumo Executivo de Assembleias:** A IA processa pautas e atas extensas das assembleias e gera resumos rápidos dos pontos e deliberações mais importantes para o morador.

---

## 🚪 4. Módulo: Access Control & Portaria

Gerencia a segurança de entrada/saída, cadastro de visitantes e o recebimento de mercadorias.

### 🔒 Funcionalidades Convencionais (Portal da Portaria & App)
* **FN-ACC-01 | Controle de Visitantes e Prestadores:** Registro manual/digital de entrada e saída de pessoas e veículos.
* **FN-ACC-02 | Protocolo de Encomendas:** Registro do pacote na portaria e notificação ao morador.

### 🤖 Integração com IA (WhatsApp & Chatbot)
* **FN-ACC-IA01 | Pré-Autorização Rápida de Visitantes:** O morador autoriza a entrada via texto no WhatsApp (*"Liberar entregador do Mercado Livre agora"*), acionando a função `AuthorizeGuest(nome, documento, data)` para liberar na portaria.
* **FN-ACC-IA02 | Notificação Inteligente de Encomendas (OCR + IA):** A portaria fotografa a etiqueta do pacote e a IA faz a leitura OCR do nome/apartamento, disparando a notificação automática para o celular do morador.

---

## 💬 5. Módulo: WhatsApp Engine & AI Orchestrator

Infraestrutura de integração responsável por intermediar as conversas e ações inteligentes.

* **FN-WPP-01 | Recepção Assíncrona de Webhooks:** Endpoint de alta performance para ingestão de mensagens do WhatsApp e enfileiramento via MassTransit/RabbitMQ.
* **FN-WPP-02 | Roteamento de Intenções (NLU/LLM):** Classificação da intenção do usuário (`GET_BOLETO`, `RESERVE_AREA`, `AUTHORIZE_GUEST`, `FAQ_QUERY`, `OPEN_TICKET`).
* **FN-WPP-03 | Execução Segura de Tools (Function Calling):** Orquestração tipada pelo Semantic Kernel para invocar microsserviços do backend .NET 10.
* **FN-WPP-04 | Gestão de Sessão e Contexto de Conversa:** Armazenamento em memória (Redis) do histórico do chat para manter a continuidade das conversas.