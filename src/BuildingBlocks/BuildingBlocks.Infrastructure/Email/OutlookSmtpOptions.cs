namespace BuildingBlocks.Infrastructure.Email;

/// <summary>
/// Opções de configuração para o cliente SMTP do Microsoft Outlook / Office 365.
/// </summary>
public sealed class OutlookSmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// Servidor SMTP (Padrão: smtp.office365.com).
    /// </summary>
    public string Host { get; set; } = "smtp.office365.com";

    /// <summary>
    /// Porta SMTP (Padrão: 587 para STARTTLS).
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Nome de usuário do e-mail do Outlook (ex: notificacoes@condominio.com).
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha da conta ou Senha de Aplicativo (App Password).
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Endereço de e-mail do remetente padrão.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome de exibição do remetente padrão.
    /// </summary>
    public string FromName { get; set; } = "Smart Condo Notificações";

    /// <summary>
    /// Habilita TLS / STARTTLS no aperto de mão (Padrão: true).
    /// </summary>
    public bool EnableStartTls { get; set; } = true;

    /// <summary>
    /// Tempo limite de conexão em milissegundos (Padrão: 15000ms / 15s).
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 15000;

    /// <summary>
    /// Número máximo de tentativas em caso de erro transitório de rede.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
}
