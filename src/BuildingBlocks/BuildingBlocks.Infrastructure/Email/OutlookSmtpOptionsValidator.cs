using Microsoft.Extensions.Options;

namespace BuildingBlocks.Infrastructure.Email;

/// <summary>
/// Validador das opções de configuração do SMTP do Microsoft Outlook executado no startup da aplicação.
/// </summary>
public sealed class OutlookSmtpOptionsValidator : IValidateOptions<OutlookSmtpOptions>
{
    public ValidateOptionsResult Validate(string? name, OutlookSmtpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Host))
        {
            errors.Add("Smtp:Host não pode ser vazio.");
        }

        if (options.Port <= 0 || options.Port > 65535)
        {
            errors.Add("Smtp:Port deve ser um número entre 1 e 65535.");
        }

        if (string.IsNullOrWhiteSpace(options.Username))
        {
            errors.Add("Smtp:Username é obrigatório para autenticação SMTP.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            errors.Add("Smtp:Password é obrigatório para autenticação SMTP.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail) || !options.FromEmail.Contains('@'))
        {
            errors.Add("Smtp:FromEmail deve ser um endereço de e-mail válido.");
        }

        if (errors.Count > 0)
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}
