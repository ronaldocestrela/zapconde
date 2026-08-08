using BuildingBlocks.Shared.Email;
using BuildingBlocks.Shared.Events;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Modules.Identity.Application.Services;

namespace Tests.Unit.Identity;

public sealed class TenantOnboardingEmailUnitTests
{
    [Fact]
    public void BuildWelcomeEmail_Should_ConstructValidEmailMessage()
    {
        // Arrange
        var email = "sindico@condominioexemplo.com.br";
        var name = "Carlos Silva";
        var condo = "Residencial Vista Verde";
        var tenantId = 42;
        var tempPassword = "Zap@TempPass123";

        // Act
        var emailMessage = TenantWelcomeEmailBuilder.BuildWelcomeEmail(email, name, condo, tenantId, tempPassword);

        // Assert
        emailMessage.Should().NotBeNull();
        emailMessage.To.Should().ContainSingle().Which.Should().Be(email);
        emailMessage.Subject.Should().Contain("Bem-vindo ao SmartCondo");
        emailMessage.Subject.Should().Contain(condo);
        
        emailMessage.BodyText.Should().Contain(name);
        emailMessage.BodyText.Should().Contain(condo);
        emailMessage.BodyText.Should().Contain(tenantId.ToString());
        emailMessage.BodyText.Should().Contain(tempPassword);

        emailMessage.BodyHtml.Should().Contain(name);
        emailMessage.BodyHtml.Should().Contain(condo);
        emailMessage.BodyHtml.Should().Contain(tenantId.ToString());
        emailMessage.BodyHtml.Should().Contain(tempPassword);
    }

    [Fact]
    public async Task SendEmailConsumer_Should_CallEmailService_WhenCommandReceived()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService
            .Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildingBlocks.Shared.Result.Success("E-mail enviado com sucesso."));

        var mockLogger = new Mock<ILogger<BuildingBlocks.Infrastructure.Email.SendEmailConsumer>>();
        var consumer = new BuildingBlocks.Infrastructure.Email.SendEmailConsumer(mockEmailService.Object, mockLogger.Object);

        var emailMessage = TenantWelcomeEmailBuilder.BuildWelcomeEmail(
            "admin@condo.com", "Admin", "Condo Teste", 1, "Zap@123");
        var command = new SendEmailCommand(emailMessage, tenantId: 1);

        var mockConsumeContext = new Mock<ConsumeContext<SendEmailCommand>>();
        mockConsumeContext.Setup(c => c.Message).Returns(command);
        mockConsumeContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        // Act
        await consumer.Consume(mockConsumeContext.Object);

        // Assert
        mockEmailService.Verify(x => x.SendEmailAsync(
            It.Is<EmailMessage>(m => m.To.Contains("admin@condo.com")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
