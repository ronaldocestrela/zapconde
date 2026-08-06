# language: pt-BR
Funcionalidade: Plano de Manutenção Preventiva e Alertas Automáticos
  Como Síndico ou Zelador do condomínio
  Quero cadastrar e acompanhar o calendário de manutenções preventivas de elevadores, bombas e para-raios
  Para evitar falhas operacionais e garantir o cumprimento de prazos regulatórios

  Cenario: Cadastro de plano de manutenção para elevadores com status em dia
    Dado que o síndico cadastra um plano de manutenção para o equipamento "Elevador Social Bloco A"
    E seleciona a categoria "Elevadores" e a periodicidade "Mensal"
    E a data da próxima manutenção é definida para 30 dias no futuro
    Quando o plano de manutenção é processado
    Então o status da manutenção deve ser "EmDia"
    E o plano deve possuir isolamento de condomínio e tenant

  Cenario: Identificação de alerta automático quando a manutenção estiver nos próximos 15 dias
    Dado que existe um plano de manutenção para "Inspeção de Bombas d'Água"
    E a data da próxima manutenção está agendada para 7 dias no futuro
    Quando a regra de alerta de manutenção é avaliada
    Então o status da manutenção deve ser alterado para "Proxima"

  Cenario: Identificação de manutenção atrasada quando o prazo expirar
    Dado que existe um plano de manutenção para "Vistoria Anual de Para-raios"
    E a data da próxima manutenção era de 2 dias atrás
    Quando a regra de alerta de manutenção é avaliada
    Então o status da manutenção deve ser alterado para "Atrasada"

  Cenario: Conclusão de manutenção preventiva com cálculo automático da próxima data
    Dado que existe um plano de manutenção para "Manutenção Preventiva de Geradores" com periodicidade "Semestral"
    E o status atual é "Atrasada"
    Quando o zelador registra a baixa da manutenção com a data de realização de hoje e custo real de "1500.00"
    Então o status da manutenção deve ser atualizado para "EmDia"
    E a data da última manutenção deve ser hoje
    E a nova data da próxima manutenção deve ser avançada em 6 meses
