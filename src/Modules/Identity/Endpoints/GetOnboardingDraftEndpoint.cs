using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

public sealed class GetOnboardingDraftRequest
{
    public Guid DraftId { get; set; }
}

/// <summary>
/// Recupera rascunho salvo do wizard de onboarding.
/// </summary>
public sealed class GetOnboardingDraftEndpoint : Endpoint<GetOnboardingDraftRequest, Result<OnboardingDraftDto>>
{
    private readonly IOnboardingDraftService _draftService;

    public GetOnboardingDraftEndpoint(IOnboardingDraftService draftService) => _draftService = draftService;

    public override void Configure()
    {
        Get("/api/tenants/onboarding/draft/{DraftId}");
        AllowAnonymous();
        Summary(s => s.Summary = "Recuperar rascunho do onboarding");
    }

    public override async Task HandleAsync(GetOnboardingDraftRequest req, CancellationToken ct)
    {
        var result = await _draftService.GetDraftAsync(req.DraftId, ct);
        await SendAsync(result, result.IsSuccess ? 200 : 404, ct);
    }
}
