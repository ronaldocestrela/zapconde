namespace Modules.Identity.Domain;

/// <summary>
/// Administradora de condomínios — representa o tenant raiz (TenantId = Id).
/// </summary>
public class Administradora
{
    public int Id { get; private set; }

    public string RazaoSocial { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string NomeFantasia { get; private set; } = string.Empty;

    public LicensePlan LicensePlan { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ICollection<Condominio> Condominios { get; private set; } = [];

    private Administradora() { }

    public static Administradora Create(
        int id,
        string razaoSocial,
        string cnpj,
        string nomeFantasia,
        LicensePlan licensePlan)
    {
        var normalizedCnpj = CnpjValidator.Normalize(cnpj);
        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            throw new DomainValidationException("Razão social é obrigatória.");
        }

        if (!CnpjValidator.IsValid(normalizedCnpj))
        {
            throw new DomainValidationException("CNPJ inválido.");
        }

        if (string.IsNullOrWhiteSpace(nomeFantasia))
        {
            throw new DomainValidationException("Nome fantasia é obrigatório.");
        }

        var now = DateTime.UtcNow;
        return new Administradora
        {
            Id = id,
            RazaoSocial = razaoSocial.Trim(),
            Cnpj = normalizedCnpj,
            NomeFantasia = nomeFantasia.Trim(),
            LicensePlan = licensePlan,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
