using BuildingBlocks.Shared;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Application;

public interface IOnboardingDraftService
{
    Task<Result<OnboardingDraftSaveResultDto>> SaveDraftAsync(OnboardingDraftDto draft, CancellationToken ct = default);

    Task<Result<OnboardingDraftDto>> GetDraftAsync(Guid draftId, CancellationToken ct = default);

    Task RemoveDraftAsync(Guid draftId, CancellationToken ct = default);
}

public interface ICnpjLookupService
{
    Task<Result<CnpjStatusDto>> GetStatusAsync(string cnpj, CancellationToken ct = default);
}

public interface ICepLookupService
{
    Task<Result<CepLookupDto>> LookupAsync(string cep, CancellationToken ct = default);
}

public interface ITenantOnboardingService
{
    Task<Result<TenantCreatedDto>> CreateAsync(CreateTenantRequestDto request, CancellationToken ct = default);
}
