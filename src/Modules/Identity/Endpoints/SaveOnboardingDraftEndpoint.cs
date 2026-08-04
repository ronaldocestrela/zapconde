using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Salva ou atualiza rascunho do wizard de onboarding de tenant.
/// </summary>
public sealed class SaveOnboardingDraftEndpoint : Endpoint<OnboardingDraftDto, Result<OnboardingDraftSaveResultDto>>
{
    private readonly IOnboardingDraftService _draftService;

    public SaveOnboardingDraftEndpoint(IOnboardingDraftService draftService) => _draftService = draftService;

    public override void Configure()
    {
        Post("/api/tenants/onboarding/draft");
        AllowAnonymous();
        Summary(s => s.Summary = "Salvar rascunho do onboarding");
    }

    public override async Task HandleAsync(OnboardingDraftDto req, CancellationToken ct)
    {
        var result = await _draftService.SaveDraftAsync(req, ct);
        await SendAsync(result, result.IsSuccess ? 200 : 400, ct);
    }
}
