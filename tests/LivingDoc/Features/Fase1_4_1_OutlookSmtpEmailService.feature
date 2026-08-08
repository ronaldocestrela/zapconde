# language: pt-BR
Funcionalidade: Envio de e-mails via cliente SMTP Microsoft Outlook

  Como sistema de notificacoes do SmartCondo
  Quero enviar e-mails transacionais e comunicados via SMTP do Microsoft Outlook
  Para garantir comunicacao confiavel com moradores, sindicos e administradoras

  Contexto:
    Dado que a solucao utiliza .NET 10 e Clean Architecture
    E o servico de e-mail utiliza a biblioteca MailKit/MimeKit com suporte a STARTTLS na porta 587
    E todas as respostas de operacao utilizam o Result Pattern de BuildingBlocks.Shared

  Cenario: Validacao de configuracoes obrigatorias do SMTP do Outlook no startup
    Dado que o IConfiguration contem a secao "Smtp"
    Quando a secao "Smtp" possuir Host, Port, Username, Password e FromEmail validos
    Entao a aplicacao deve registrar IEmailService com a implementacao OutlookSmtpEmailService
    E a validacao de opcoes deve ter sucesso

  Cenario: Falha de inicializacao quando credenciais SMTP sao invalidas ou ausentes
    Dado que a secao "Smtp" no appsettings contem Host ou Username em branco
    Quando a aplicacao inicializar os servicos de infraestrutura
    Entao a validacao de opcoes deve falhar com mensagem explicativa

  Cenario: Envio de e-mail HTML com anexo utilizando Result Pattern com sucesso
    Dado um e-mail com destinatario "morador@condominio.com", assunto "Sua Fatura Chegou" e corpo HTML
    E um anexo PDF contendo o boleto condominial
    Quando o IEmailService enviar a mensagem
    Entao a operacao deve retornar um Result com IsSuccess igual a true
    E a mensagem deve ser convertida com sucesso em MimeMessage

  Cenario: Tratamento resiliente de erro de conexao SMTP retornando Result Failure
    Dado que o servidor SMTP do Outlook esteja inacessivel ou recusar a conexao
    Quando o IEmailService tentar enviar o e-mail
    Entao a operacao deve capturar a excecao de rede/autenticacao
    E deve retornar um Result com IsSuccess igual a false
    E a mensagem de erro deve conter detalhes compreensiveis sem expor a stack trace

  Cenario: Processamento assincrono de e-mail via mensageria MassTransit SendEmailCommand
    Dado um comando SendEmailCommand publicado no bus do MassTransit
    Quando o SendEmailConsumer consumir a mensagem da fila RabbitMQ
    Entao o IEmailService deve ser acionado para processar o envio
    E o processamento deve ser finalizado sem erros
