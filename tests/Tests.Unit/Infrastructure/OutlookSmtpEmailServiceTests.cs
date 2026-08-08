using MailKit;
using BuildingBlocks.Infrastructure.Email;
using BuildingBlocks.Shared.Email;
using BuildingBlocks.Shared.Events;
using FluentAssertions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;

namespace Tests.Unit.Infrastructure;

public class OutlookSmtpEmailServiceTests
{
    private readonly OutlookSmtpOptions _validOptions = new()
    {
        Host = "smtp.office365.com",
        Port = 587,
        Username = "teste@office365.com",
        Password = "SecretPassword123",
        FromEmail = "teste@office365.com",
        FromName = "Smart Condo Notificações Teste",
        EnableStartTls = true
    };

    [Fact]
    public void OutlookSmtpOptionsValidator_ShouldValidateSuccessfully_WhenAllFieldsAreValid()
    {
        // Arrange
        var validator = new OutlookSmtpOptionsValidator();

        // Act
        var result = validator.Validate("Smtp", _validOptions);

        // Assert
        result.Failed.Should().BeFalse();
    }

    [Fact]
    public void OutlookSmtpOptionsValidator_ShouldFail_WhenUsernameOrPasswordIsMissing()
    {
        // Arrange
        var validator = new OutlookSmtpOptionsValidator();
        var invalidOptions = new OutlookSmtpOptions
        {
            Host = "smtp.office365.com",
            Port = 587,
            Username = "",
            Password = "",
            FromEmail = "invalid"
        };

        // Act
        var result = validator.Validate("Smtp", invalidOptions);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(f => f.Contains("Username"));
        result.Failures.Should().Contain(f => f.Contains("Password"));
    }

    [Fact]
    public void BuildMimeMessage_ShouldConvertEmailMessageCorrectly_WithAttachmentsAndHeaders()
    {
        // Arrange
        var optionsMock = Options.Create(_validOptions);
        var loggerMock = new Mock<ILogger<OutlookSmtpEmailService>>();
        var service = new OutlookSmtpEmailService(optionsMock, loggerMock.Object);

        var attachmentBytes = "Conteudo em PDF"u8.ToArray();
        var attachment = new EmailAttachment("boleto_123.pdf", attachmentBytes, "application/pdf");

        var emailMsg = new EmailMessage(
            to: new[] { "morador@gmail.com" },
            subject: "Boleto do Mês de Agosto",
            bodyHtml: "<h1>Segue o boleto em anexo</h1>",
            bodyText: "Segue o boleto em anexo",
            from: "financeiro@zapcondo.com.br",
            fromName: "Financeiro SmartCondo",
            cc: new[] { "sindico@gmail.com" },
            bcc: new[] { "auditoria@zapcondo.com.br" },
            replyTo: "suporte@zapcondo.com.br",
            attachments: new[] { attachment });

        // Act
        var mimeMsg = service.BuildMimeMessage(emailMsg);

        // Assert
        mimeMsg.Should().NotBeNull();
        mimeMsg.Subject.Should().Be("Boleto do Mês de Agosto");
        mimeMsg.From.Mailboxes.First().Address.Should().Be("financeiro@zapcondo.com.br");
        mimeMsg.To.Mailboxes.First().Address.Should().Be("morador@gmail.com");
        mimeMsg.Cc.Mailboxes.First().Address.Should().Be("sindico@gmail.com");
        mimeMsg.Bcc.Mailboxes.First().Address.Should().Be("auditoria@zapcondo.com.br");
        mimeMsg.ReplyTo.Mailboxes.First().Address.Should().Be("suporte@zapcondo.com.br");
        mimeMsg.Body.Should().NotBeNull();
    }

    [Fact]
    public async Task SendEmailAsync_ShouldConnectAndSendEmail_WhenSmtpClientSucceeds()
    {
        // Arrange
        var optionsMock = Options.Create(_validOptions);
        var loggerMock = new Mock<ILogger<OutlookSmtpEmailService>>();
        var smtpClientMock = new Mock<ISmtpClient>();

        var service = new OutlookSmtpEmailService(optionsMock, loggerMock.Object, () => smtpClientMock.Object);
        var emailMsg = new EmailMessage("destinatario@condominio.com", "Teste SMTP", "<p>Corpo de teste</p>");

        // Act
        var result = await service.SendEmailAsync(emailMsg);

        // Assert
        result.IsSuccess.Should().BeTrue();
        smtpClientMock.Verify(s => s.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls, It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(s => s.AuthenticateAsync("teste@office365.com", "SecretPassword123", It.IsAny<CancellationToken>()), Times.Once);
        smtpClientMock.Verify(s => s.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()), Times.Once);
        smtpClientMock.Verify(s => s.DisconnectAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldReturnResultFailure_WhenSmtpThrowsException()
    {
        // Arrange
        var optionsMock = Options.Create(_validOptions);
        var loggerMock = new Mock<ILogger<OutlookSmtpEmailService>>();
        var smtpClientMock = new Mock<ISmtpClient>();

        smtpClientMock.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SecureSocketOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SmtpCommandException(SmtpErrorCode.UnexpectedStatusCode, SmtpStatusCode.TransactionFailed, "STARTTLS is required."));

        var service = new OutlookSmtpEmailService(optionsMock, loggerMock.Object, () => smtpClientMock.Object);
        var emailMsg = new EmailMessage("destinatario@condominio.com", "Teste Erro", "<p>Erro</p>");

        // Act
        var result = await service.SendEmailAsync(emailMsg);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("STARTTLS is required");
    }

    [Fact]
    public async Task SendEmailConsumer_ShouldInvokeEmailService_WhenConsumingSendEmailCommand()
    {
        // Arrange
        var emailServiceMock = new Mock<IEmailService>();
        emailServiceMock.Setup(s => s.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildingBlocks.Shared.Result.Success());

        var loggerMock = new Mock<ILogger<SendEmailConsumer>>();
        var consumer = new SendEmailConsumer(emailServiceMock.Object, loggerMock.Object);

        var command = new SendEmailCommand(new EmailMessage("morador@test.com", "Comunicado"), tenantId: 1);
        var consumeContextMock = new Mock<ConsumeContext<SendEmailCommand>>();
        consumeContextMock.Setup(c => c.Message).Returns(command);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        emailServiceMock.Verify(s => s.SendEmailAsync(It.Is<EmailMessage>(m => m.Subject == "Comunicado"), It.IsAny<CancellationToken>()), Times.Once);
    }
}
