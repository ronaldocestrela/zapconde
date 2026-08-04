using BuildingBlocks.Shared.MultiTenancy;

namespace Modules.Identity.Domain;

/// <summary>
/// Condomínio gerenciado por uma administradora (CondoId = Id, TenantId = Administradora.Id).
/// </summary>
public class Condominio : ITenantScoped
{
    public int Id { get; private set; }

    public int TenantId { get; set; }

    public string Nome { get; private set; } = string.Empty;

    public CondominioTipo Tipo { get; private set; }

    public int TotalUnits { get; private set; }

    public int NumberOfBlocks { get; private set; }

    public Endereco Endereco { get; private set; } = new();

    public string MasterAdminName { get; private set; } = string.Empty;

    public string CorporateEmail { get; private set; } = string.Empty;

    public string PhoneWhatsApp { get; private set; } = string.Empty;

    public string EmergencyPhone { get; private set; } = string.Empty;

    public ConfiguracoesIniciais Configuracoes { get; private set; } = new();

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public Administradora? Administradora { get; private set; }

    private Condominio() { }

    public static Condominio Create(
        int id,
        int tenantId,
        string nome,
        CondominioTipo tipo,
        int totalUnits,
        int numberOfBlocks,
        Endereco endereco,
        string masterAdminName,
        string corporateEmail,
        string phoneWhatsApp,
        string emergencyPhone,
        ConfiguracoesIniciais configuracoes)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainValidationException("Nome do condomínio é obrigatório.");
        }

        if (totalUnits < 1)
        {
            throw new DomainValidationException("Total de unidades deve ser maior que zero.");
        }

        if (numberOfBlocks < 1)
        {
            throw new DomainValidationException("Número de blocos deve ser maior que zero.");
        }

        if (!Endereco.IsValidCep(endereco.Cep))
        {
            throw new DomainValidationException("CEP inválido.");
        }

        if (string.IsNullOrWhiteSpace(endereco.Logradouro) ||
            string.IsNullOrWhiteSpace(endereco.Cidade) ||
            string.IsNullOrWhiteSpace(endereco.Uf))
        {
            throw new DomainValidationException("Endereço incompleto.");
        }

        if (string.IsNullOrWhiteSpace(masterAdminName))
        {
            throw new DomainValidationException("Nome do responsável é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(corporateEmail) || !corporateEmail.Contains('@'))
        {
            throw new DomainValidationException("E-mail corporativo inválido.");
        }

        ConfiguracoesIniciais.Validate(configuracoes.DiaVencimento);

        var now = DateTime.UtcNow;
        return new Condominio
        {
            Id = id,
            TenantId = tenantId,
            Nome = nome.Trim(),
            Tipo = tipo,
            TotalUnits = totalUnits,
            NumberOfBlocks = numberOfBlocks,
            Endereco = endereco,
            MasterAdminName = masterAdminName.Trim(),
            CorporateEmail = corporateEmail.Trim().ToLowerInvariant(),
            PhoneWhatsApp = phoneWhatsApp?.Trim() ?? string.Empty,
            EmergencyPhone = emergencyPhone?.Trim() ?? string.Empty,
            Configuracoes = configuracoes,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
