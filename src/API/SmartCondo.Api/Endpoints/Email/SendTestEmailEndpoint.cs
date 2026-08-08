using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Email;
using FastEndpoints;

namespace SmartCondo.Api.Endpoints.Email;

public record SendTestEmailRequest(
    string To,
    string Subject = "E-mail de Teste - Smart Condo SaaS",
    string? BodyHtml = "<h1>Smart Condo SaaS</h1><p>E-mail de teste enviado com sucesso via <strong>SMTP Microsoft Outlook</strong>.</p>");

/// <summary>
/// Endpoint administrativo/desenvolvimento para testar a conectividade e disparo de e-mails via SMTP do Outlook.
/// </summary>
public sealed class SendTestEmailEndpoint : Endpoint<SendTestEmailRequest, Result<string>>
{
    private readonly IEmailService _emailService;

    public SendTestEmailEndpoint(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public override void Configure()
    {
        Post("/api/v1/email/send-test");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Enviar e-mail de teste";
            s.Description = "Dispara um e-mail de teste via cliente SMTP do Microsoft Outlook para validar credenciais e conectividade.";
        });
    }

    public override async Task HandleAsync(SendTestEmailRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.To))
        {
            await SendAsync(Result<string>.ValidationFailure(new[] { "O destinatário (To) é obrigatório." }), 400, ct);
            return;
        }

        var message = new EmailMessage(
            to: req.To,
            subject: req.Subject,
            bodyHtml: req.BodyHtml ?? "<p>Teste</p>",
            bodyText: "E-mail de teste enviado com sucesso via SMTP Microsoft Outlook.");

        var result = await _emailService.SendEmailAsync(message, ct);

        if (result.IsSuccess)
        {
            await SendAsync(Result<string>.Success($"E-mail de teste enviado com sucesso para {req.To}"), 200, ct);
        }
        else
        {
            await SendAsync(Result<string>.Failure(result.Message), 400, ct);
        }
    }
}
