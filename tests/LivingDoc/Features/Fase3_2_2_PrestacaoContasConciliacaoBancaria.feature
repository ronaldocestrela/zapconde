# language: pt-BR
Funcionalidade: Prestacao de Contas Digital e Conciliacao Bancaria Multicondominio

  Como síndico ou administradora condominial
  Quero gerar pastas digitais de prestação de contas, realizar conciliação bancária de extratos e visualizar relatórios financeiros consolidados
  Para garantir a transparência financeira, controle bancário e governança do condomínio com isolamento multi-tenant

  Cenário: Fechamento de Pasta Digital de Prestação de Contas mensal e submissão ao conselho fiscal
    Dado que o condomínio "1" possui balancete registrado para o mês "7" de "2026" com receitas de "15000.00" e despesas de "12000.00"
    Quando o síndico solicita a geração da pasta digital de prestação de contas para o mês "7" de "2026"
    Então a pasta digital deve ser criada com status "Rascunho" e saldo do mês calculado em "3000.00"
    Quando o síndico submete a pasta digital para apreciação do conselho fiscal
    Então o status da pasta digital deve ser alterado para "EmAnaliseConselho"
    E a aprovação pelo conselho deve alterar o status para "Aprovada"

  Cenário: Conciliação bancária automática de extrato bancário por matching de data e valor
    Dado que a conta bancária do condomínio "1" possui um lançamento de crédito no extrato de "500.00" recebido em "2026-07-10"
    E que existe uma fatura de taxa condominial paga no sistema no valor exato de "500.00" na mesma data
    Quando o motor de conciliação bancária é executado
    Então o lançamento do extrato deve ser marcado com status "ConciliadoAutomatico"
    E o score de correspondência da conciliação deve ser de "100" por cento

  Cenário: Conciliação manual de lançamento pendente no extrato bancário
    Dado que a conta bancária do condomínio "1" possui um lançamento de débito de "150.00" sem correspondência automática
    Quando a administradora realiza a conciliação manual vinculando o lançamento a um item de despesa de manutenção
    Então o status de conciliação do item do extrato deve ser atualizado para "ConciliadoManual"
    E o registro de conciliação bancária deve ficar vinculado à despesa correspondente

  Cenário: Consolidação de relatórios multicondomínio para visão da administradora
    Dado que o Tenant "1" gerencia os condomínios "Condomínio Flores" e "Condomínio Sol"
    Quando a administradora solicita o relatório financeiro consolidado multicondomínio
    Então o relatório deve retornar o somatório total de receitas, despesas e a taxa média de inadimplência de todos os condomínios do Tenant

  Cenário: Garantia de isolamento multi-tenant entre pastas digitais e extratos bancários
    Dado que existem pastas digitais registradas no condomínio "1" (Tenant "1")
    Quando a administradora consulta pastas digitais autenticada sob o Tenant "2"
    Então a consulta deve retornar uma lista vazia sem expor dados do Tenant "1"
