using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

/// <summary>
/// Cria tenant completo (administradora + condomínio + usuário master) via wizard de onboarding.
/// </summary>
public sealed class CreateTenantOnboardingEndpoint : Endpoint<CreateTenantRequestDto, Result<TenantCreatedDto>>
{
    private readonly ITenantOnboardingService _onboardingService;

    public CreateTenantOnboardingEndpoint(ITenantOnboardingService onboardingService) =>
        _onboardingService = onboardingService;

    public override void Configure()
    {
        Post("/api/tenants/onboarding");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Criar tenant (administradora + condomínio)";
            s.Description = "Executa criação transacional de administradora, condomínio e membership master.";
        });
    }

    public override async Task HandleAsync(CreateTenantRequestDto req, CancellationToken ct)
    {
        var result = await _onboardingService.CreateAsync(req, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 201, ct);
            return;
        }

        var status = result.Message.Contains("CNPJ já cadastrado", StringComparison.OrdinalIgnoreCase) ? 409
            : result.Message.Contains("rollback", StringComparison.OrdinalIgnoreCase) ? 500
            : result.Message.Contains("validação", StringComparison.OrdinalIgnoreCase) ||
              result.Errors.Any() ? 422
            : 400;

        await SendAsync(result, status, ct);
    }
}
