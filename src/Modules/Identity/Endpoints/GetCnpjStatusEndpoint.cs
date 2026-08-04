using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

public sealed class GetCnpjStatusRequest
{
    public string Cnpj { get; set; } = string.Empty;
}

/// <summary>
/// Verifica disponibilidade de CNPJ para cadastro de administradora.
/// </summary>
public sealed class GetCnpjStatusEndpoint : Endpoint<GetCnpjStatusRequest, Result<CnpjStatusDto>>
{
    private readonly ICnpjLookupService _cnpjLookupService;

    public GetCnpjStatusEndpoint(ICnpjLookupService cnpjLookupService) => _cnpjLookupService = cnpjLookupService;

    public override void Configure()
    {
        Get("/api/tenants/cnpj/{Cnpj}/status");
        AllowAnonymous();
        Summary(s => s.Summary = "Status de disponibilidade do CNPJ");
    }

    public override async Task HandleAsync(GetCnpjStatusRequest req, CancellationToken ct)
    {
        var result = await _cnpjLookupService.GetStatusAsync(req.Cnpj, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, 200, ct);
            return;
        }

        var status = result.Message.Contains("cadastrado", StringComparison.OrdinalIgnoreCase) ? 409
            : result.Message.Contains("validação", StringComparison.OrdinalIgnoreCase) ||
              result.Errors.Any() ? 422
            : 400;

        await SendAsync(result, status, ct);
    }
}
