# language: pt-BR
Funcionalidade: Prevenção de Colisão de Reservas de Áreas Comuns com Redis Distributed Lock

  Como morador ou gestor condominial
  Quero efetuar reservas de áreas comuns sem que haja colisão de horários simultâneos no mesmo espaço
  Para garantir transparência, evitar sobreposição de eventos e assegurar alta concorrência assíncrona com Redis Lock

  Cenário: Reserva realizada com sucesso em horário livre e dentro do horário de funcionamento
    Dado que a área comum "Salão de Festas Principal" está "Ativa" com horário das "08:00" às "22:00"
    E o morador "10" solicita uma reserva para o dia de amanhã das "14:00" às "18:00" com "30" pessoas
    Quando o serviço de agendamento de reserva é executado
    Então a reserva deve ser criada com sucesso com o valor total calculado
    E o status da reserva deve ser "Confirmada"

  Cenário: Bloqueio de colisão quando já existe reserva confirmada no mesmo intervalo
    Dado que a área comum "Churrasqueira VIP" possui uma reserva confirmada das "12:00" às "16:00" no dia "2026-08-10"
    Quando outro morador tenta reservar a mesma "Churrasqueira VIP" das "14:00" às "18:00" no dia "2026-08-10"
    Então a solicitação deve ser rejeitada por colisão de horário
    E nenhuma nova reserva deve ser inserida

  Cenário: Concorrência assíncrona tratada com Redis Distributed Lock
    Dado que dois moradores tentam agendar simultaneamente a mesma área comum "Salão de Festas Principal" para o exato mesmo horário
    Quando o Redis Lock orquestra as requisições paralelas
    Então exatamente uma requisição deve obter sucesso com código de criação
    E a requisição concorrente deve receber resposta de conflito

  Cenário: Tentativa de reserva em área em manutenção ou fora do horário de funcionamento
    Dado que a área comum "Quadra Poliesportiva" está com status "Manutencao"
    Quando o morador tenta efetuar uma reserva para esta quadra
    Então a operação deve falhar informando que a área não está ativa para reservas

  Cenário: Garantia de isolamento multi-tenant nas reservas de condomínios distintos
    Dado que o Tenant "1" possui uma reserva na área comum "Salão de Festas Principal"
    Quando o usuário do Tenant "2" solicita a listagem de reservas
    Então as reservas do Tenant "1" não devem ser expostas para o Tenant "2"
