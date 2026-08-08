using BuildingBlocks.Shared.Email;
using BuildingBlocks.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BuildingBlocks.Infrastructure.Email;

/// <summary>
/// Serviço de envio de e-mails via cliente SMTP do Microsoft Outlook / Office 365.
/// </summary>
public sealed class OutlookSmtpEmailService : IEmailService
{
    private readonly OutlookSmtpOptions _options;
    private readonly ILogger<OutlookSmtpEmailService> _logger;
    private readonly Func<ISmtpClient> _smtpClientFactory;

    public OutlookSmtpEmailService(
        IOptions<OutlookSmtpOptions> options,
        ILogger<OutlookSmtpEmailService> logger,
        Func<ISmtpClient>? smtpClientFactory = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
        _smtpClientFactory = smtpClientFactory ?? (() => new SmtpClient());
    }

    public async Task<Result> SendEmailAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (message is null)
        {
            return Result.ValidationFailure(new[] { "A mensagem de e-mail não pode ser nula." });
        }

        if (message.To == null || message.To.Count == 0)
        {
            return Result.ValidationFailure(new[] { "É necessário informar ao menos um destinatário no campo To." });
        }

        try
        {
            var mimeMessage = BuildMimeMessage(message);

            using var client = _smtpClientFactory();
            client.Timeout = _options.TimeoutMilliseconds;

            var secureOptions = _options.EnableStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            _logger.LogInformation(
                "Conectando ao servidor SMTP {Host}:{Port} para enviar e-mail para {To}",
                _options.Host, _options.Port, string.Join(", ", message.To));

            await client.ConnectAsync(_options.Host, _options.Port, secureOptions, ct);
            await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            await client.SendAsync(mimeMessage, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("E-mail com assunto '{Subject}' enviado com sucesso via SMTP Outlook.", message.Subject);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail com assunto '{Subject}' via SMTP Outlook.", message.Subject);
            return Result.Failure($"Falha ao enviar e-mail via SMTP Outlook: {ex.Message}");
        }
    }

    public async Task<Result> SendBulkEmailAsync(IEnumerable<EmailMessage> messages, CancellationToken ct = default)
    {
        if (messages is null)
        {
            return Result.ValidationFailure(new[] { "A lista de mensagens de e-mail não pode ser nula." });
        }

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            return Result.Success();
        }

        var errors = new List<string>();
        var successCount = 0;

        foreach (var msg in messageList)
        {
            var result = await SendEmailAsync(msg, ct);
            if (result.IsSuccess)
            {
                successCount++;
            }
            else
            {
                errors.Add($"[Assunto: {msg.Subject}] {result.Message}");
            }
        }

        if (errors.Count > 0)
        {
            _logger.LogWarning("Envio em lote concluído com {SuccessCount} sucessos e {ErrorCount} falhas.", successCount, errors.Count);
            return Result.Failure($"Falha no envio de {errors.Count} e-mail(s) do lote: {string.Join(" | ", errors)}");
        }

        return Result.Success();
    }

    /// <summary>
    /// Converte um EmailMessage de domínio em MimeMessage do MimeKit.
    /// </summary>
    public MimeMessage BuildMimeMessage(EmailMessage message)
    {
        var mimeMessage = new MimeMessage();

        var fromEmail = !string.IsNullOrWhiteSpace(message.From) ? message.From : _options.FromEmail;
        var fromName = !string.IsNullOrWhiteSpace(message.FromName) ? message.FromName : _options.FromName;

        mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));

        foreach (var recipient in message.To)
        {
            mimeMessage.To.Add(MailboxAddress.Parse(recipient));
        }

        foreach (var ccRecipient in message.Cc)
        {
            mimeMessage.Cc.Add(MailboxAddress.Parse(ccRecipient));
        }

        foreach (var bccRecipient in message.Bcc)
        {
            mimeMessage.Bcc.Add(MailboxAddress.Parse(bccRecipient));
        }

        if (!string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            mimeMessage.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
        }

        mimeMessage.Subject = message.Subject;

        var builder = new BodyBuilder();

        if (!string.IsNullOrWhiteSpace(message.BodyHtml))
        {
            builder.HtmlBody = message.BodyHtml;
        }

        if (!string.IsNullOrWhiteSpace(message.BodyText))
        {
            builder.TextBody = message.BodyText;
        }

        if (message.Attachments != null)
        {
            foreach (var attachment in message.Attachments)
            {
                var contentType = ContentType.Parse(attachment.ContentType);
                builder.Attachments.Add(attachment.FileName, attachment.Content, contentType);
            }
        }

        mimeMessage.Body = builder.ToMessageBody();
        return mimeMessage;
    }
}
