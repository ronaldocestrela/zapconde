# language: pt-BR
Funcionalidade: Function Calling e Plugin Semantic Kernel de Consulta de Boletos Pendentes
  Como morador do condomínio
  Quero solicitar informações sobre minhas cobranças e faturas pendentes via linguagem natural no chat
  Para receber instantaneamente a chave PIX Copia e Cola, linha digitável e o link do PDF do boleto em aberto

  Cenário: Consulta bem-sucedida de boletos pendentes via Plugin do Semantic Kernel
    Dado que o morador de ID "10" está cadastrado no condomínio "1"
    E possui uma fatura pendente de "250.00" com código PIX "00020126580014br.gov.bcb.pix0136zapcondo-pix-1-fat1" e PDF "/api/financial/invoices/1/pdf"
    Quando o agente de IA executa a intenção "Consultar boletos pendentes" invocando a tool "GetPendingBoletos" com moradorId "10"
    Então o plugin deve retornar a lista de faturas e boletos em aberto contendo o valor "250.00", a chave PIX e o link do PDF
    E o status da resposta do Result Pattern deve indicar sucesso com dados encapsulados

  Cenário: Consulta de morador sem débitos pendentes
    Dado que o morador de ID "20" está em dia com todas as suas obrigações financeiras no condomínio "1"
    Quando a tool "GetPendingBoletos" é invocada para o moradorId "20"
    Então o plugin deve retornar uma resposta indicando que não existem boletos ou cobranças pendentes para o morador
    E o indicador de adimplência deve ser confirmed

  Cenário: Garantia de isolamento multi-tenant na consulta de boletos
    Dado que existe uma fatura pendente do morador "30" no condomínio "2"
    Quando o contexto da requisição é do condomínio "1" e tenta-se consultar o morador "30"
    Então a consulta do plugin não deve retornar boletos de outros condomínios devido ao filtro de isolamento por tenantId
