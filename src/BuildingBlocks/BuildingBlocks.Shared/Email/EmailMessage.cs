namespace BuildingBlocks.Shared.Email;

/// <summary>
/// Representa a mensagem de e-mail a ser enviada.
/// </summary>
public sealed record EmailMessage
{
    /// <summary>
    /// Lista de e-mails dos destinatários principais.
    /// </summary>
    public IReadOnlyList<string> To { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Assunto do e-mail.
    /// </summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>
    /// Corpo da mensagem em HTML.
    /// </summary>
    public string? BodyHtml { get; init; }

    /// <summary>
    /// Corpo da mensagem em texto puro (fallback).
    /// </summary>
    public string? BodyText { get; init; }

    /// <summary>
    /// E-mail do remetente (opcional, sobrescreve a configuração padrão).
    /// </summary>
    public string? From { get; init; }

    /// <summary>
    /// Nome exibido do remetente (opcional, sobrescreve a configuração padrão).
    /// </summary>
    public string? FromName { get; init; }

    /// <summary>
    /// Lista de e-mails em cópia (CC).
    /// </summary>
    public IReadOnlyList<string> Cc { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Lista de e-mails em cópia oculta (BCC).
    /// </summary>
    public IReadOnlyList<string> Bcc { get; init; } = Array.Empty<string>();

    /// <summary>
    /// E-mail de resposta (Reply-To).
    /// </summary>
    public string? ReplyTo { get; init; }

    /// <summary>
    /// Anexos de e-mail.
    /// </summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; init; } = Array.Empty<EmailAttachment>();

    /// <summary>
    /// Construtor simplificado para e-mail único.
    /// </summary>
    public EmailMessage(string to, string subject, string? bodyHtml = null, string? bodyText = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to, nameof(to));
        ArgumentException.ThrowIfNullOrWhiteSpace(subject, nameof(subject));

        To = new[] { to };
        Subject = subject;
        BodyHtml = bodyHtml;
        BodyText = bodyText;
    }

    /// <summary>
    /// Construtor completo com múltiplos destinatários.
    /// </summary>
    public EmailMessage(
        IEnumerable<string> to,
        string subject,
        string? bodyHtml = null,
        string? bodyText = null,
        string? from = null,
        string? fromName = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? replyTo = null,
        IEnumerable<EmailAttachment>? attachments = null)
    {
        ArgumentNullException.ThrowIfNull(to, nameof(to));
        ArgumentException.ThrowIfNullOrWhiteSpace(subject, nameof(subject));

        var toList = to.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (toList.Count == 0)
        {
            throw new ArgumentException("A lista de destinatários (To) não pode ser vazia.", nameof(to));
        }

        To = toList;
        Subject = subject;
        BodyHtml = bodyHtml;
        BodyText = bodyText;
        From = from;
        FromName = fromName;
        Cc = cc?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? (IReadOnlyList<string>)Array.Empty<string>();
        Bcc = bcc?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? (IReadOnlyList<string>)Array.Empty<string>();
        ReplyTo = replyTo;
        Attachments = attachments?.ToList() ?? (IReadOnlyList<EmailAttachment>)Array.Empty<EmailAttachment>();
    }
}
