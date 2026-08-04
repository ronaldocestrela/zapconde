using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Caching;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Infrastructure.Services;

public sealed class OnboardingDraftService(ICacheService cacheService) : IOnboardingDraftService
{
    private static readonly TimeSpan DraftTtl = TimeSpan.FromDays(7);
    private const string KeyPrefix = "onboarding:draft:";

    public async Task<Result<OnboardingDraftSaveResultDto>> SaveDraftAsync(OnboardingDraftDto draft, CancellationToken ct = default)
    {
        if (draft.DraftId == Guid.Empty)
        {
            draft.DraftId = Guid.NewGuid();
        }

        draft.SavedAt = DateTime.UtcNow;
        await cacheService.SetAsync($"{KeyPrefix}{draft.DraftId}", draft, DraftTtl, ct);

        return Result<OnboardingDraftSaveResultDto>.Success(new OnboardingDraftSaveResultDto
        {
            DraftId = draft.DraftId,
            SavedAt = draft.SavedAt
        }, "Rascunho salvo com sucesso.");
    }

    public async Task<Result<OnboardingDraftDto>> GetDraftAsync(Guid draftId, CancellationToken ct = default)
    {
        var draft = await cacheService.GetAsync<OnboardingDraftDto>($"{KeyPrefix}{draftId}", ct);
        if (draft is null)
        {
            return Result<OnboardingDraftDto>.Failure("Rascunho não encontrado.");
        }

        return Result<OnboardingDraftDto>.Success(draft);
    }

    public Task RemoveDraftAsync(Guid draftId, CancellationToken ct = default) =>
        cacheService.RemoveAsync($"{KeyPrefix}{draftId}", ct);
}
