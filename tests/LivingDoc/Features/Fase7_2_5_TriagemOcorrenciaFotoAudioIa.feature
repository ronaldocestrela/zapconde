# language: pt-BR
Funcionalidade: Triagem Inteligente de Ocorrencias via Foto e Audio com Abertura Automatica de Chamados (Semantic Kernel IA)

  Como morador ou gestor condominial
  Quero enviar uma foto ou relato em audio/texto de um problema no condominio
  Para que a IA realize a triagem automatica, categorizacao, definicao de prioridade, identificacao do local e abertura automatica do chamado

  Cenario: Morador envia foto de infiltração na garagem e a IA realiza a triagem e abre chamado de Manutencao com prioridade Alta
    Dado que o morador "Mora-G1" do condomínio "1" envia uma foto com URL "https://storage.smartcondo.com/evidences/infiltracao-garagem-b2.jpg" e o relato "Infiltração com pingos constantes caindo sobre a vaga 42 no subsolo 2"
    Quando a IA executa a ferramenta "TriarEAbrirOcorrencia" do plugin de triagem de ocorrências
    Então a IA deve classificar a ocorrência com categoria "Manutencao" e prioridade "Alta"
    E deve sugerir a localização "Subsolo 2 - Vaga 42" e o setor responsável "Zeladoria / Manutenção Predial"
    E deve criar o chamado com status "Aberta" registrando o nível de confiança superior a "0.85"
    E deve garantir que a ocorrência esteja isolada com o tenant_id do morador "1"

  Cenario: Morador envia relato em áudio sobre som alto de madrugada e a IA categoriza como Barulho com prioridade Media
    Dado que o morador "Mora-A5" do condomínio "1" envia a transcrição de áudio "Música alta e gritaria no apartamento 504 passando da 1h da manhã"
    Quando a IA executa a ferramenta "TriarEAbrirOcorrencia" para a transcrição de áudio
    Então a IA deve classificar a ocorrência com categoria "Barulho" e prioridade "Media"
    E deve definir o título "Som alto e perturbação no Bloco A Ap 504" e indicar o setor responsável "Administração / Portaria"
    E o registro no banco deve conter a origem da triagem como "IA_Audio"

  Cenario: Simulação de análise prévia multimodal sem persistência imediata no banco
    Dado que o usuário envia uma foto com a URL "https://storage.smartcondo.com/evidences/lampada-queimada-hall.jpg"
    Quando a IA executa a análise prévia multimodal da ocorrência
    Então o sistema deve retornar o DTO de triagem contendo a categoria "Manutencao", prioridade "Baixa" e a sugestão de título "Lâmpada queimada no hall do 3º andar"
    E nenhum registro de ocorrência deve ser persistido no banco até a confirmação do usuário
