# language: pt-BR
Funcionalidade: Cadastro e Controle de Fluxo de Visitantes e Prestadores de Serviços

  Como porteiro, morador ou síndico do condomínio
  Quero autorizar, registrar entrada e registrar saída de visitantes sociais e prestadores de serviços
  Para manter o controle de acesso seguro, auditável e em tempo real na portaria

  Cenario: Morador autoriza antecipadamente um visitante social
    Dado que o morador da unidade "101" cadastra a autorizacao para "Carlos Silva" com documento "123.456.789-00" e tipo "VisitanteSocial"
    Quando a autorização é salva no sistema
    Então o status da autorização deve ser "Agendado"
    E a data de inicio da permissão deve estar ativa

  Cenario: Portaria registra a entrada de um prestador de serviço
    Dado que existe uma autorização agendada para o prestador "João Eletricista" da empresa "TechFix"
    Quando a portaria confirma os dados e clica em "Registrar Entrada"
    Então o status deve transicionar para "Presente"
    E a data e hora de entrada deve ser registrada com o horário atual

  Cenario: Portaria registra a saída do visitante
    Dado que o visitante "Carlos Silva" possui status "Presente" no condomínio
    Quando a portaria clica em "Registrar Saida"
    Então o status deve transicionar para "Finalizado"
    E a data e hora de saída deve ser preenchida

  Cenario: Cancelamento de autorização agendada
    Dado que existe uma autorização com status "Agendado" para "Visitante Teste"
    Quando o morador ou a portaria solicita o cancelamento
    Então o status da autorização deve mudar para "Cancelado"
    E a entrada não deve mais ser permitida
