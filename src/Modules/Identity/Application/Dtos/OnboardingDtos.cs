using Modules.Identity.Domain;

namespace Modules.Identity.Application.Dtos;

public sealed class OnboardingAdministradoraDto
{
    public string RazaoSocial { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;

    public string NomeFantasia { get; set; } = string.Empty;

    public LicensePlan LicensePlan { get; set; } = LicensePlan.Starter;
}

public sealed class OnboardingCondominioDto
{
    public string Nome { get; set; } = string.Empty;

    public CondominioTipo Tipo { get; set; } = CondominioTipo.Residencial;

    public int TotalUnits { get; set; }

    public int NumberOfBlocks { get; set; } = 1;
}

public sealed class OnboardingEnderecoDto
{
    public string Cep { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;
}

public sealed class OnboardingContatosDto
{
    public string MasterAdminName { get; set; } = string.Empty;

    public string CorporateEmail { get; set; } = string.Empty;

    public string PhoneWhatsApp { get; set; } = string.Empty;

    public string EmergencyPhone { get; set; } = string.Empty;

    public string MasterRole { get; set; } = SmartCondoRoles.Sindico;
}

public sealed class OnboardingConfiguracoesDto
{
    public int DiaVencimento { get; set; } = 10;

    public bool JurosEnabled { get; set; }

    public bool MultaEnabled { get; set; }

    public BankGateway BankGateway { get; set; } = BankGateway.None;

    public bool WhatsAppAiEnabled { get; set; }
}

public sealed class OnboardingDraftDto
{
    public Guid DraftId { get; set; }

    public OnboardingAdministradoraDto Administradora { get; set; } = new();

    public OnboardingCondominioDto Condominio { get; set; } = new();

    public OnboardingEnderecoDto Endereco { get; set; } = new();

    public OnboardingContatosDto Contatos { get; set; } = new();

    public OnboardingConfiguracoesDto Configuracoes { get; set; } = new();

    public int CurrentStep { get; set; } = 1;

    public DateTime SavedAt { get; set; }
}

public sealed class OnboardingDraftSaveResultDto
{
    public Guid DraftId { get; set; }

    public DateTime SavedAt { get; set; }
}

public sealed class CnpjStatusDto
{
    public string Cnpj { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }

    public string Status { get; set; } = string.Empty;
}

public sealed class CepLookupDto
{
    public string Cep { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;
}

public sealed class TenantCreatedDto
{
    public int TenantId { get; set; }

    public int CondoId { get; set; }

    public string MasterEmail { get; set; } = string.Empty;

    public string CredentialsDispatchedMessage { get; set; } = string.Empty;

    public string CondominioNome { get; set; } = string.Empty;
}

public sealed class CreateTenantRequestDto
{
    public Guid? DraftId { get; set; }

    public OnboardingAdministradoraDto Administradora { get; set; } = new();

    public OnboardingCondominioDto Condominio { get; set; } = new();

    public OnboardingEnderecoDto Endereco { get; set; } = new();

    public OnboardingContatosDto Contatos { get; set; } = new();

    public OnboardingConfiguracoesDto Configuracoes { get; set; } = new();

    public bool SimulateRollback { get; set; }
}
