using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Identity.Domain;

/// <summary>
/// Cadastro condominial de morador (independente de ApplicationUser).
/// </summary>
public class Morador : ITenantScoped
{
    public int Id { get; private set; }

    public int TenantId { get; set; }

    public int CondoId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    public string Cpf { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string TelefoneWhatsApp { get; private set; } = string.Empty;

    public string? TelefoneWhatsAppE164 { get; private set; }

    public PhoneVerificationStatus PhoneVerificationStatus { get; private set; }

    public DateTime? PhoneVerifiedAtUtc { get; private set; }

    public DateTime? PhoneVerificationRequestedAtUtc { get; private set; }

    public Guid? UserId { get; private set; }

    public ApplicationUser? User { get; private set; }

    public ICollection<VinculoUnidade> Vinculos { get; private set; } = [];

    private Morador() { }

    public static Morador Create(
        int tenantId,
        int condoId,
        string nome,
        string cpf,
        string email,
        string telefoneWhatsApp,
        Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainValidationException("Nome do morador é obrigatório.");
        }

        var cpfNormalizado = CpfValidator.Normalize(cpf);
        if (!CpfValidator.IsValid(cpfNormalizado))
        {
            throw new DomainValidationException("CPF inválido.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new DomainValidationException("E-mail inválido.");
        }

        return new Morador
        {
            TenantId = tenantId,
            CondoId = condoId,
            Nome = nome.Trim(),
            Cpf = cpfNormalizado,
            Email = email.Trim().ToLowerInvariant(),
            TelefoneWhatsApp = telefoneWhatsApp?.Trim() ?? string.Empty,
            TelefoneWhatsAppE164 = PhoneNumberValidator.TryNormalizeBrazilianMobile(telefoneWhatsApp, out var e164)
                ? e164
                : null,
            PhoneVerificationStatus = PhoneVerificationStatus.NaoInformado,
            UserId = userId
        };
    }

    public void Atualizar(string nome, string cpf, string email, string telefoneWhatsApp)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainValidationException("Nome do morador é obrigatório.");
        }

        var cpfNormalizado = CpfValidator.Normalize(cpf);
        if (!CpfValidator.IsValid(cpfNormalizado))
        {
            throw new DomainValidationException("CPF inválido.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new DomainValidationException("E-mail inválido.");
        }

        Nome = nome.Trim();
        Cpf = cpfNormalizado;
        Email = email.Trim().ToLowerInvariant();
        TelefoneWhatsApp = telefoneWhatsApp?.Trim() ?? string.Empty;

        var normalized = PhoneNumberValidator.TryNormalizeBrazilianMobile(telefoneWhatsApp, out var e164)
            ? e164
            : null;
        if (!string.Equals(TelefoneWhatsAppE164, normalized, StringComparison.Ordinal))
        {
            TelefoneWhatsAppE164 = normalized;
            PhoneVerificationStatus = PhoneVerificationStatus.NaoInformado;
            PhoneVerifiedAtUtc = null;
            PhoneVerificationRequestedAtUtc = null;
        }
    }

    public void IniciarVerificacaoTelefone(string telefoneE164, DateTime requestedAtUtc)
    {
        TelefoneWhatsAppE164 = PhoneNumberValidator.NormalizeBrazilianMobile(telefoneE164);
        TelefoneWhatsApp = PhoneNumberValidator.FormatBrazilianMobile(TelefoneWhatsAppE164);
        PhoneVerificationStatus = PhoneVerificationStatus.AguardandoValidacao;
        PhoneVerificationRequestedAtUtc = requestedAtUtc;
        PhoneVerifiedAtUtc = null;
    }

    public void ConfirmarTelefone(DateTime verifiedAtUtc)
    {
        if (PhoneVerificationStatus != PhoneVerificationStatus.AguardandoValidacao ||
            string.IsNullOrWhiteSpace(TelefoneWhatsAppE164))
        {
            throw new DomainValidationException("Não existe uma verificação de celular pendente.");
        }

        PhoneVerificationStatus = PhoneVerificationStatus.Validado;
        PhoneVerifiedAtUtc = verifiedAtUtc;
    }

    public void MarcarCodigoExpirado()
    {
        PhoneVerificationStatus = PhoneVerificationStatus.Expirado;
        PhoneVerifiedAtUtc = null;
    }
}
