namespace BuildingBlocks.Shared.Email;

/// <summary>
/// Representa um anexo de e-mail.
/// </summary>
public sealed record EmailAttachment
{
    /// <summary>
    /// Nome do arquivo com extensão (ex: "boleto.pdf").
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Conteúdo do arquivo em bytes.
    /// </summary>
    public byte[] Content { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Tipo MIME do arquivo (ex: "application/pdf", "image/png").
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// Cria um novo anexo de e-mail.
    /// </summary>
    public EmailAttachment(string fileName, byte[] content, string contentType = "application/octet-stream")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentNullException.ThrowIfNull(content, nameof(content));

        FileName = fileName;
        Content = content;
        ContentType = contentType;
    }
}
