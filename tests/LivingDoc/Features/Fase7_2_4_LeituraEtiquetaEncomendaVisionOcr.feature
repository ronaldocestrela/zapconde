# language: pt-BR
Funcionalidade: Leitura Inteligente de Etiquetas de Encomenda via Vision OCR e Notificacao Automatica no Semantic Kernel

  Como operador de portaria ou zelador do condomínio
  Quero capturar ou enviar fotos de etiquetas de encomendas recebidas
  Para que o assistente de IA extraia automaticamente destinatário, código de rastreio e transportadora, cadastre a encomenda e notifique o morador via WhatsApp

  Cenário: Leitura de foto de etiqueta com extração completa dos dados e alta confiança
    Dado que o operador de portaria do condomínio "1" possui uma foto de etiqueta de encomenda
    Quando a API de visão computacional processa a imagem da etiqueta da transportadora "Mercado Livre" para o destinatário "Apto 102 - Bloco A"
    Então os dados extraídos devem conter a unidade "Bloco A - Apto 102", transportadora "Mercado Livre" e grau de confiança superior a "85%"
    E a encomenda deve ser registrada com status "AguardandoRetirada" no módulo de controle de acesso
    E o registro da encomenda deve ser vinculado ao Tenant "1" do condomínio

  Cenário: Leitura de etiqueta com registro automático e notificação instantânea ao morador
    Dado que a imagem da etiqueta do pacote "TRK-99887766" do morador da unidade "204" é processada via assistente de IA
    Quando o Plugin "PackageVisionPlugin" executa a ferramenta "ReadPackageLabelAndNotify"
    Então o sistema deve salvar a encomenda no banco de dados do módulo de portaria
    E a encomenda deve ter a propriedade "NotificadoEm" preenchida com o horário atual de notificação ao morador
    E o retorno da IA deve confirmar o envio da notificação via WhatsApp para o morador cadastrado

  Cenário: Tentativa de associação de encomenda com imagem de etiqueta de outro condomínio (Isolamento Multi-Tenant)
    Dado que o operador do condomínio "1" tenta processar uma etiqueta para uma unidade que pertence ao condomínio "2"
    Quando a ferramenta de Visão/OCR realiza o cruzamento de dados com a base de moradores
    Então o sistema não deve permitir o vínculo com unidades de outros condomínios mantendo o filtro rigoroso de TenantId
