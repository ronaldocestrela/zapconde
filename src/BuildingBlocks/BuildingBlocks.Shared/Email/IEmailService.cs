namespace BuildingBlocks.Shared.Email;

/// <summary>
/// Contrato do serviço de e-mail da aplicação.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia um e-mail de forma assíncrona retornando o Result com o estado da operação.
    /// </summary>
    /// <param name="message">Dados da mensagem de e-mail.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Result de sucesso ou falha.</returns>
    Task<Result> SendEmailAsync(EmailMessage message, CancellationToken ct = default);

    /// <summary>
    /// Envia múltiplos e-mails em lote de forma assíncrona.
    /// </summary>
    /// <param name="messages">Coleção de mensagens a serem enviadas.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Result de sucesso ou falha contendo detalhes de mensagens com erro.</returns>
    Task<Result> SendBulkEmailAsync(IEnumerable<EmailMessage> messages, CancellationToken ct = default);
}
