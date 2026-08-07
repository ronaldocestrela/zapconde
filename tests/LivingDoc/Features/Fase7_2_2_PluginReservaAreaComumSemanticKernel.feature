# language: pt-BR
Funcionalidade: Function Calling e Plugin Semantic Kernel de Reserva de Áreas Comuns
  Como morador do condomínio
  Quero solicitar o agendamento de uma área comum (ex: Salão de Festas, Churrasqueira) via atendimento inteligente por chat
  Para validar disponibilidade, regras de funcionamento, capacidade e efetuar o agendamento automático

  Cenário: Reserva bem-sucedida de área comum via Plugin do Semantic Kernel
    Dado que o morador de ID "10" do condomínio "1" solicita a reserva da área comum "Salão de Festas" (ID 1)
    E a área comum está ativa com capacidade para "50" pessoas e requer aprovação do síndico
    Quando a tool "ReserveCommonArea" é executada com a data "2026-09-15 18:00" às "2026-09-15 22:00" para "25" pessoas
    Então o plugin deve efetuar o agendamento com status "PendenteAprovacao" e calcular as taxas correspondentes
    E o payload JSON retornado para a LLM deve conter a confirmação e o número da reserva criada

  Cenário: Rejeição de reserva por colisão de horário com reserva existente
    Dado que a área comum "Churrasqueira" (ID 2) já possui uma reserva confirmada no dia "2026-09-20 12:00" às "2026-09-20 16:00"
    Quando outro morador tenta invocar a tool "ReserveCommonArea" para o mesmo período de horário na área ID "2"
    Então o plugin deve barrar o agendamento devido ao bloqueio distribuído de concorrência e sobreposição
    E deve retornar uma mensagem amigável de erro informando a indisponibilidade do horário solicitado

  Cenário: Listagem prévia de áreas comuns disponíveis para atendimento da IA
    Dado que o condomínio "1" possui áreas comuns cadastradas como "Salão de Festas", "Churrasqueira" e "Quadra"
    Quando a tool "GetAvailableCommonAreas" é invocada pelo Semantic Kernel
    Então o plugin deve retornar o catálogo de áreas comuns ativas com seus respectivos IDs, horários e capacidades
