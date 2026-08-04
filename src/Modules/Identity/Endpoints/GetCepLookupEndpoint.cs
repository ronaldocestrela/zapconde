using BuildingBlocks.Shared;
using FastEndpoints;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Endpoints;

public sealed class GetCepLookupRequest
{
    public string Cep { get; set; } = string.Empty;
}

/// <summary>
/// Consulta endereço por CEP (ViaCEP ou stub em ambiente de testes).
/// </summary>
public sealed class GetCepLookupEndpoint : Endpoint<GetCepLookupRequest, Result<CepLookupDto>>
{
    private readonly ICepLookupService _cepLookupService;

    public GetCepLookupEndpoint(ICepLookupService cepLookupService) => _cepLookupService = cepLookupService;

    public override void Configure()
    {
        Get("/api/tenants/cep/{Cep}");
        AllowAnonymous();
        Summary(s => s.Summary = "Consulta de endereço por CEP");
    }

    public override async Task HandleAsync(GetCepLookupRequest req, CancellationToken ct)
    {
        var result = await _cepLookupService.LookupAsync(req.Cep, ct);
        await SendAsync(result, result.IsSuccess ? 200 : result.Errors.Any() ? 422 : 404, ct);
    }
}
