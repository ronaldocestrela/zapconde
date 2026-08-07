# language: pt-BR
Funcionalidade: Registro e Baixa de Encomendas na Portaria

  Como porteiro, morador ou síndico do condomínio
  Quero registrar o recebimento de encomendas e dar baixa quando o morador retirar
  Para manter o controle de correspondências organizado, auditável e notificar os moradores

  Cenario: Portaria registra o recebimento de uma encomenda
    Dado que a portaria recebe uma caixa da transportadora "Loggi" para a unidade "202"
    E o código de rastreio é "LOG123456789"
    Quando o recebimento da encomenda é cadastrado no sistema
    Então o status da encomenda deve ser "AguardandoRetirada"
    E a data de recebimento deve ser registrada com o horário atual
    E a unidade "202" deve constar com encomenda pendente

  Cenario: Notificação do morador sobre encomenda disponível
    Dado que existe uma encomenda com status "AguardandoRetirada" para a unidade "202"
    Quando a portaria aciona a notificação ao morador
    Então o sistema deve registrar a data e hora da notificação
    E a notificação deve ser enviada com sucesso

  Cenario: Morador faz a retirada da encomenda na portaria
    Dado que a encomenda "LOG123456789" está com status "AguardandoRetirada"
    Quando o morador "Roberto Alves" retira a encomenda na portaria
    Então o status da encomenda deve transicionar para "Entregue"
    E a data e hora de retirada devem ser preenchidas
    E o nome de quem retirou deve ser registrado como "Roberto Alves"

  Cenario: Tentativa inválida de dar baixa em encomenda já entregue
    Dado que a encomenda "LOG123456789" possui status "Entregue"
    Quando é feita uma nova tentativa de dar baixa nessa encomenda
    Então o sistema deve recusar a operação e retornar erro de negócio
