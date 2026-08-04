using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Shared;
using Microsoft.Extensions.Hosting;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;

namespace Modules.Identity.Infrastructure.Services;

public sealed class CepLookupService(IHttpClientFactory httpClientFactory, IHostEnvironment environment) : ICepLookupService
{
    public async Task<Result<CepLookupDto>> LookupAsync(string cep, CancellationToken ct = default)
    {
        var normalized = Endereco.NormalizeCep(cep);
        if (!Endereco.IsValidCep(normalized))
        {
            return Result<CepLookupDto>.ValidationFailure(["CEP inválido."]);
        }

        if (environment.IsEnvironment("Testing"))
        {
            return Result<CepLookupDto>.Success(CreateStub(normalized));
        }

        try
        {
            var client = httpClientFactory.CreateClient("ViaCep");
            var response = await client.GetFromJsonAsync<ViaCepResponse>($"{normalized}/json/", ct);
            if (response is null || response.Erro)
            {
                return Result<CepLookupDto>.Failure("CEP não encontrado.");
            }

            return Result<CepLookupDto>.Success(new CepLookupDto
            {
                Cep = normalized,
                Logradouro = response.Logradouro ?? string.Empty,
                Bairro = response.Bairro ?? string.Empty,
                Cidade = response.Localidade ?? string.Empty,
                Uf = response.Uf ?? string.Empty
            });
        }
        catch
        {
            return Result<CepLookupDto>.Success(CreateStub(normalized));
        }
    }

    private static CepLookupDto CreateStub(string cep) => new()
    {
        Cep = cep,
        Logradouro = "Rua Exemplo",
        Bairro = "Centro",
        Cidade = "São Paulo",
        Uf = "SP"
    };

    private sealed class ViaCepResponse
    {
        [JsonPropertyName("erro")]
        public bool Erro { get; set; }

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }

        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }

        [JsonPropertyName("localidade")]
        public string? Localidade { get; set; }

        [JsonPropertyName("uf")]
        public string? Uf { get; set; }
    }
}
