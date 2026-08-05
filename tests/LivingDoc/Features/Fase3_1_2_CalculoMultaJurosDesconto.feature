# language: pt-BR
Funcionalidade: Motor Financeiro de Calculo de Multas, Juros Pro-Rata e Desconto ate o Vencimento
  Como sindico, morador ou administradora do condominio
  Eu quero simular e calcular encargos por atraso e descontos de pontualidade
  Para garantir transparencia no valor a ser pago e auditoria automatizada do calculo

  Contexto:
    Dado que o motor de calculo financeiro esta ativo no sistema
    E que o condomínio padrao aplica multa de 2.0% por atraso e juros de mora de 1.0% ao mes pro-rata dia

  Cenario: Pagamento realizado ate a data de vencimento aplica desconto de pontualidade
    Dado que uma fatura possui valor original de "500.00" e vencimento em "2026-08-10"
    E possui desconto de pontualidade configurado em "20.00"
    Quando a data de simulação de pagamento e "2026-08-10"
    Entao a multa deve ser "0.00"
    E os juros pro-rata devem ser "0.00"
    E o desconto aplicado deve ser "20.00"
    E o valor total a pagar deve ser "480.00"

  Cenario: Pagamento realizado com 10 dias de atraso aplica multa de 2% e juros pro-rata dia
    Dado que uma fatura possui valor original de "1000.00" e vencimento em "2026-08-01"
    E nao possui desconto de pontualidade
    Quando a data de simulação de pagamento e "2026-08-11"
    Entao os dias em atraso devem ser 10
    E o valor da multa de 2% deve ser "20.00"
    E os juros pro-rata de 1% a.m. (0.0333% ao dia) por 10 dias devem ser "3.33"
    E o desconto aplicado deve ser "0.00"
    E o valor total a pagar deve ser "1023.33"
    E a memoria de calculo textual deve conter a trilha de auditoria dos juros e multa

  Cenario: Simulação ad-hoc de encargos via API POST /api/financial/simulator/calculate
    Quando eu envio um payload de simulação com valor "750.00", vencimento "2026-08-01", simulação "2026-08-16", multa "2.0%" e juros "1.0%"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve conter "isSuccess" igual a "true"
    E a resposta deve retornar os dias em atraso igual a 15
    E o valor total retornado deve conter o valor corrigido com multa e juros pro-rata de 15 dias

  Cenario: Obter projecao futura de encargos para uma fatura existente via GET /api/financial/invoices/{id}/projection
    Dado que existe uma fatura cadastrada com ID valido
    Quando eu envio GET para "/api/financial/invoices/{id}/projection"
    Entao o status HTTP deve ser 200 OK
    E a resposta deve retornar uma lista de projecoes para 0, 7, 15, 30 e 60 dias de atraso

  Cenario: Tentar simular com dados invalidos como valor negativo retorna 400 Bad Request
    Quando eu envio um payload de simulação com valor "-100.00"
    Entao o status HTTP deve ser 400 Bad Request
    E a resposta deve indicar erro de validação via Result Pattern
