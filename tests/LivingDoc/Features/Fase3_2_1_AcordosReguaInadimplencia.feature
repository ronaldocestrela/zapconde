# language: pt-BR
Funcionalidade: Acordos de Renegociacao e Regua de Inadimplencia

  Como síndico ou administradora condominial
  Quero simular e registrar acordos de renegociação de débitos e gerenciar a régua de cobrança de inadimplentes
  Para recuperar valores em atraso e manter a saúde financeira do condomínio com isolamento multi-tenant

  Cenário: Simulação e efetivação de acordo de renegociação com parcelamento e abatimento de encargos
    Dado que a unidade "101" do condomínio "1" possui 2 faturas vencidas no valor total original de "500.00"
    Quando o síndico solicita uma simulação de acordo concedendo "50.00" de desconto e parcelamento em "2" vezes
    Então a simulação deve retornar o valor total consolidado do acordo em "450.00" com parcelas de "225.00"
    Quando o síndico efetiva a proposta de acordo para a unidade "101"
    Então o acordo deve ser registrado com status "Ativo"
    E as faturas originais devem ter seus status atualizados para "EmAcordo"

  Cenário: Descumprimento de acordo ao atrasar pagamento de parcela
    Dado que a unidade "102" do condomínio "1" possui um acordo "Ativo" com 3 parcelas
    Quando a primeira parcela do acordo vence e não é paga após o prazo de tolerância
    Então o status do acordo deve ser alterado para "Descumprido"
    E as faturas originais consolidadas devem retornar para o status "Vencido"

  Cenário: Execução do motor de régua de inadimplência para faturas em atraso
    Dado que o condomínio "1" possui réguas de cobrança configuradas para D+3 "LembreteAmigavel" e D+10 "NotificacaoCobranca"
    E que a unidade "103" possui uma fatura vencida há 12 dias
    Quando o motor de régua de inadimplência é executado
    Então o sistema deve registrar um histórico de cobrança para a unidade "103" na etapa "NotificacaoCobranca"
    E o histórico deve conter o canal "WhatsApp" e status de envio confirmado

  Cenário: Garantia de isolamento multi-tenant entre acordos e réguas de cobrança
    Dado que existem acordos registrados no condomínio "1" (Tenant "1")
    Quando a administradora consulta acordos filtrados pelo Tenant "2"
    Então a consulta deve retornar uma lista vazia sem expor dados do Tenant "1"
