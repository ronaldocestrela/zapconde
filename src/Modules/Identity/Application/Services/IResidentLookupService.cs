namespace Modules.Identity.Application.Services;

public record ResidentLookupResultDto(
    int TenantId,
    int CondoId,
    int MoradorId,
    Guid? UserId,
    string Nome,
    string TelefoneWhatsAppE164);

public interface IResidentLookupService
{
    /// <summary>
    /// Busca um morador ativo pelo seu número de telefone no formato E.164 (ex: +5575999999999).
    /// Suporta busca global ou restrita ao tenant.
    /// </summary>
    Task<ResidentLookupResultDto?> FindByPhoneE164Async(string phoneE164, int? tenantId = null, CancellationToken cancellationToken = default);
}
