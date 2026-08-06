# language: pt-BR
Funcionalidade: Gestao de Areas Comuns com Regras de Capacidade e Custos

  Como síndico ou administradora condominial
  Quero cadastrar e gerenciar áreas comuns (Salão de Festas, Churrasqueira, Quadras, etc.) com regras de capacidade, custos e horários
  Para garantir o controle de uso, precificação transparente e governança no condomínio com isolamento multi-tenant

  Cenário: Cadastro de nova área comum com capacidade, taxas e horários válidos
    Dado que o síndico do condomínio "1" deseja cadastrar a área comum "Salão de Festas Principal"
    E define o tipo como "Eventos", capacidade máxima de "100" pessoas e taxa de reserva de "150.00" com taxa de limpeza de "50.00"
    E define o horário de funcionamento das "08:00" às "22:00" com antecedência mínima de "2" dias
    Quando o serviço de cadastro de área comum é executado
    Então a área comum deve ser criada com status "Ativa" e custo total calculado em "200.00"

  Cenário: Tentativa de cadastro de área comum com capacidade ou horários inválidos
    Dado que o síndico tenta cadastrar a área comum "Churrasqueira VIP" com capacidade de "0" pessoas
    Quando o serviço de cadastro de área comum é executado
    Então a operação deve retornar falha de validação informando que a capacidade deve ser maior que zero

  Cenário: Alteração do status operacional da área comum para manutenção
    Dado que a área comum "Salão de Festas Principal" está com status "Ativa"
    Quando o síndico solicita alterar o status para "Manutencao"
    Então o status da área comum deve ser atualizado para "Manutencao" e ela não deve aceitar novas reservas

  Cenário: Atualização de regras de custos e taxa de limpeza da área comum
    Dado que a área comum "Salão de Festas Principal" possui taxa de reserva de "150.00" e limpeza de "50.00"
    Quando o síndico atualiza a taxa de reserva para "200.00" e a limpeza para "60.00"
    Então o custo total recalculado da área comum deve ser de "260.00"

  Cenário: Garantia de isolamento multi-tenant entre áreas comuns de diferentes condomínios
    Dado que o Tenant "1" possui a área comum "Salão de Festas Principal"
    Quando o usuário autenticado sob o Tenant "2" solicita a listagem de áreas comuns
    Então a consulta deve retornar uma lista vazia sem expor as áreas comuns do Tenant "1"
