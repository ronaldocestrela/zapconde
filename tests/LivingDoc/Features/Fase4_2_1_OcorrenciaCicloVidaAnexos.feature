# language: pt-BR
Funcionalidade: Gestao de Ocorrencias e Ciclo de Vida do Chamado

  Como um morador ou administrador do condominio
  Eu quero registrar chamados de ocorrencias com fotos e acompanhar o historico de status
  Para garantir a resolucao eficiente dos problemas operacionais do condominio

  Cenario: Abertura de ocorrencia com anexo de foto e estado inicial Aberta
    Dado que o morador "101" do condominio "1" deseja relatar "Vazamento na garagem do Bloco A"
    E seleciona a categoria "Manutencao" e a prioridade "Alta"
    Quando a ocorrencia e registrada no sistema com uma foto "vazamento_garagem.png"
    Entao o status inicial da ocorrencia deve ser "Aberta"
    E a ocorrencia deve possuir 1 anexo de foto cadastrado
    E deve haver 1 registro inicial no historico de auditoria com o comentario "Ocorrencia aberta pelo morador"

  Cenario: Transicao valida de ciclo de vida da ocorrencia com historico
    Dado que existe uma ocorrencia com status "Aberta" no condominio "1"
    Quando o zelador "Zelador Carlos" assume o chamado alterando o status para "EmAndamento" com o comentario "Iniciada inspecao no local"
    E posteriormente finaliza o chamado alterando o status para "Resolvida" com o comentario "Vazamento reparado com troca de vedacao"
    Entao o status final da ocorrencia deve ser "Resolvida"
    E a data de conclusao deve estar preenchida
    E o historico de auditoria deve conter 3 registros de mudanca de status

  Cenario: Impedir transicao invalida de status em ocorrencia ja resolvida
    Dado que uma ocorrencia no condominio "1" ja foi finalizada como "Resolvida"
    Quando houver uma tentativa de alterar o status diretamente para "EmAndamento"
    Entao o sistema deve rejeitar a transicao e manter o status como "Resolvida"
