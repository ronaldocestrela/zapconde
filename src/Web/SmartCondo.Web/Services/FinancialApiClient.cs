using System.Net.Http.Json;
using System.Text.Json;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Domain.Enums;

namespace SmartCondo.Web.Services;

public sealed class FinancialApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ApiResult<IEnumerable<FaturaSummaryDto>>> GetInvoicesAsync(
        int? condoId = null,
        int? unidadeId = null,
        string? competencia = null,
        StatusFatura? status = null,
        CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string>();
            if (condoId.HasValue && condoId.Value > 0) queryParams.Add($"condoId={condoId}");
            if (unidadeId.HasValue && unidadeId.Value > 0) queryParams.Add($"unidadeId={unidadeId}");
            if (!string.IsNullOrWhiteSpace(competencia)) queryParams.Add($"competencia={Uri.EscapeDataString(competencia)}");
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await httpClient.GetAsync($"/api/financial/invoices{queryString}", ct);
            return await ParseAsync<IEnumerable<FaturaSummaryDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<FaturaSummaryDto>>(ex);
        }
    }

    public async Task<ApiResult<FaturaDetailDto>> GetInvoiceByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/invoices/{id}", ct);
            return await ParseAsync<FaturaDetailDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<FaturaDetailDto>(ex);
        }
    }

    public async Task<ApiResult<FaturaDetailDto>> CreateInvoiceAsync(CreateFaturaRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/financial/invoices", request, JsonOptions, ct);
            return await ParseAsync<FaturaDetailDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<FaturaDetailDto>(ex);
        }
    }

    public async Task<ApiResult<bool>> CancelInvoiceAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/invoices/{id}/cancel", null, ct);
            var apiRes = await ParseAsync<object>(response, ct);
            return new ApiResult<bool>(apiRes.IsSuccess, apiRes.IsSuccess, apiRes.Message, apiRes.StatusCode);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<bool>(ex);
        }
    }

    public async Task<ApiResult<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>> GeneratePaymentAsync(int faturaId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/invoices/{faturaId}/generate-payment", null, ct);
            return await ParseAsync<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>(ex);
        }
    }

    public async Task<ApiResult<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>> GetPaymentInfoAsync(int faturaId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/invoices/{faturaId}/payment-info", ct);
            return await ParseAsync<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>(ex);
        }
    }

    public async Task<ApiResult<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>> SyncPaymentAsync(int faturaId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/invoices/{faturaId}/sync-payment", null, ct);
            return await ParseAsync<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<Modules.Financial.Application.Dtos.PaymentInfoResponseDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<AcordoDto>>> GetAgreementsAsync(int condoId = 1, int? unidadeId = null, StatusAcordo? status = null, CancellationToken ct = default)
    {
        try
        {
            var queryParams = new List<string> { $"condoId={condoId}" };
            if (unidadeId.HasValue) queryParams.Add($"unidadeId={unidadeId}");
            if (status.HasValue) queryParams.Add($"status={(int)status.Value}");

            var queryString = "?" + string.Join("&", queryParams);
            var response = await httpClient.GetAsync($"/api/financial/agreements{queryString}", ct);
            return await ParseAsync<IEnumerable<AcordoDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<AcordoDto>>(ex);
        }
    }

    public async Task<ApiResult<AcordoDto>> CreateAgreementAsync(CriarAcordoRequest request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/financial/agreements", request, JsonOptions, ct);
            return await ParseAsync<AcordoDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<AcordoDto>(ex);
        }
    }

    public async Task<ApiResult<bool>> CancelAgreementAsync(int id, string motivo, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/agreements/{id}/cancel?motivo={Uri.EscapeDataString(motivo)}", null, ct);
            var apiRes = await ParseAsync<object>(response, ct);
            return new ApiResult<bool>(apiRes.IsSuccess, apiRes.IsSuccess, apiRes.Message, apiRes.StatusCode);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<bool>(ex);
        }
    }

    public async Task<ApiResult<DashboardInadimplenciaDto>> GetDunningDashboardAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/dunning/dashboard?condoId={condoId}", ct);
            return await ParseAsync<DashboardInadimplenciaDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<DashboardInadimplenciaDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<EtapaReguaDto>>> GetDunningConfigAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/dunning/config?condoId={condoId}", ct);
            return await ParseAsync<IEnumerable<EtapaReguaDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<EtapaReguaDto>>(ex);
        }
    }

    public async Task<ApiResult<ProcessamentoReguaResultadoDto>> ProcessDunningAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/dunning/process?condoId={condoId}", null, ct);
            return await ParseAsync<ProcessamentoReguaResultadoDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ProcessamentoReguaResultadoDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<PastaDigitalDto>>> GetDigitalBindersAsync(int condoId = 1, int? ano = null, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/financial/digital-binders?condoId={condoId}" + (ano.HasValue ? $"&ano={ano}" : "");
            var response = await httpClient.GetAsync(url, ct);
            return await ParseAsync<IEnumerable<PastaDigitalDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<PastaDigitalDto>>(ex);
        }
    }

    public async Task<ApiResult<PastaDigitalDto>> GetDigitalBinderByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/digital-binders/{id}", ct);
            return await ParseAsync<PastaDigitalDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PastaDigitalDto>(ex);
        }
    }

    public async Task<ApiResult<PastaDigitalDto>> CreateDigitalBinderAsync(CriarPastaDigitalRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/financial/digital-binders/generate", request, JsonOptions, ct);
            return await ParseAsync<PastaDigitalDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PastaDigitalDto>(ex);
        }
    }

    public async Task<ApiResult<PastaDigitalDto>> SubmitDigitalBinderAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/digital-binders/{id}/submit", null, ct);
            return await ParseAsync<PastaDigitalDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PastaDigitalDto>(ex);
        }
    }

    public async Task<ApiResult<PastaDigitalDto>> ApproveDigitalBinderAsync(int id, AprovarPastaDigitalRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync($"/api/financial/digital-binders/{id}/approve", request, JsonOptions, ct);
            return await ParseAsync<PastaDigitalDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<PastaDigitalDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<ContaBancariaDto>>> GetBankAccountsAsync(int condoId = 1, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/bank-reconciliation/accounts?condoId={condoId}", ct);
            return await ParseAsync<IEnumerable<ContaBancariaDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ContaBancariaDto>>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<ExtratoBancarioItemDto>>> ImportBankStatementAsync(ImportarExtratoRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/financial/bank-reconciliation/import-statement", request, JsonOptions, ct);
            return await ParseAsync<IEnumerable<ExtratoBancarioItemDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ExtratoBancarioItemDto>>(ex);
        }
    }

    public async Task<ApiResult<ResultadoConciliacaoEmLoteDto>> AutoReconcileAsync(int contaBancariaId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsync($"/api/financial/bank-reconciliation/auto-reconcile/{contaBancariaId}", null, ct);
            return await ParseAsync<ResultadoConciliacaoEmLoteDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ResultadoConciliacaoEmLoteDto>(ex);
        }
    }

    public async Task<ApiResult<IEnumerable<ExtratoBancarioItemDto>>> GetPendingReconciliationItemsAsync(int contaBancariaId, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/financial/bank-reconciliation/pending-items/{contaBancariaId}", ct);
            return await ParseAsync<IEnumerable<ExtratoBancarioItemDto>>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<IEnumerable<ExtratoBancarioItemDto>>(ex);
        }
    }

    public async Task<ApiResult<ExtratoBancarioItemDto>> ManualReconcileAsync(ConciliarManualRequestDto request, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/financial/bank-reconciliation/reconcile-item", request, JsonOptions, ct);
            return await ParseAsync<ExtratoBancarioItemDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<ExtratoBancarioItemDto>(ex);
        }
    }

    public async Task<ApiResult<RelatorioConsolidadoMulticondominioDto>> GetMultiCondoSummaryReportAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync("/api/financial/reports/multi-condo-summary", ct);
            return await ParseAsync<RelatorioConsolidadoMulticondominioDto>(response, ct);
        }
        catch (Exception ex)
        {
            return ConnectionFailure<RelatorioConsolidadoMulticondominioDto>(ex);
        }
    }

    private static async Task<ApiResult<T>> ParseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        var statusCode = (int)response.StatusCode;
        var content = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(content))
        {
            return new ApiResult<T>(false, default, $"HTTP {statusCode}: resposta vazia.", statusCode);
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            bool isSuccess = root.TryGetProperty("isSuccess", out var isSuccessProp) && isSuccessProp.GetBoolean();
            string message = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() ?? string.Empty : string.Empty;

            T? data = default;
            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
            {
                data = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), JsonOptions);
            }

            return new ApiResult<T>(isSuccess && response.IsSuccessStatusCode, data, message, statusCode);
        }
        catch (JsonException ex)
        {
            return new ApiResult<T>(false, default, $"Erro ao processar JSON: {ex.Message}", statusCode);
        }
    }

    private static ApiResult<T> ConnectionFailure<T>(Exception ex) =>
        new(false, default, $"Falha de conexão com a API: {ex.Message}", 503);
}
