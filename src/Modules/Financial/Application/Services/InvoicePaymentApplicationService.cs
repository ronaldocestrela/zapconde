using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Enums;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public interface IInvoicePaymentApplicationService
{
    Task<Result<PaymentInfoResponseDto>> GeneratePaymentAsync(int faturaId, CancellationToken ct = default);
    Task<Result<PaymentInfoResponseDto>> GetPaymentInfoAsync(int faturaId, CancellationToken ct = default);
    Task<Result<PaymentInfoResponseDto>> SyncPaymentAsync(int faturaId, CancellationToken ct = default);
}

public class InvoicePaymentApplicationService : IInvoicePaymentApplicationService
{
    private readonly FinancialDbContext _dbContext;
    private readonly IPaymentGatewayService _gatewayService;
    private readonly ICurrentTenantService _tenantService;
    private readonly ILogger<InvoicePaymentApplicationService> _logger;

    public InvoicePaymentApplicationService(
        FinancialDbContext dbContext,
        IPaymentGatewayService gatewayService,
        ICurrentTenantService tenantService,
        ILogger<InvoicePaymentApplicationService> logger)
    {
        _dbContext = dbContext;
        _gatewayService = gatewayService;
        _tenantService = tenantService;
        _logger = logger;
    }

    public async Task<Result<PaymentInfoResponseDto>> GeneratePaymentAsync(int faturaId, CancellationToken ct = default)
    {
        if (faturaId <= 0)
        {
            return Result<PaymentInfoResponseDto>.ValidationFailure(new[] { "ID de fatura inválido." });
        }

        var fatura = await _dbContext.Faturas
            .Include(f => f.Boleto)
            .FirstOrDefaultAsync(f => f.Id == faturaId, ct);

        if (fatura == null)
        {
            return Result<PaymentInfoResponseDto>.Failure("Fatura não encontrada.");
        }

        // Se a fatura já possui boleto vinculado no gateway com dados válidos, retorna as informações existentes
        if (fatura.Boleto != null && !string.IsNullOrWhiteSpace(fatura.Boleto.ExternalChargeId))
        {
            return Result<PaymentInfoResponseDto>.Success(MapToPaymentInfo(fatura, fatura.Boleto));
        }

        var request = new BoletoCobrancaRequestDto(
            FaturaId: fatura.Id,
            Valor: fatura.TotalFinal,
            DataVencimento: fatura.DataVencimento,
            MoradorNome: $"Morador Unidade #{fatura.UnidadeId}",
            MoradorCpfCnpj: "00000000000",
            Descricao: $"Taxa Condominial Ref. {fatura.Competencia}"
        );

        var gatewayResult = await _gatewayService.GerarCobrancaBoletoPixAsync(request, ct);
        if (!gatewayResult.IsSuccess || gatewayResult.Data == null)
        {
            return Result<PaymentInfoResponseDto>.Failure(gatewayResult.Message ?? "Falha ao gerar cobrança no gateway.");
        }

        var data = gatewayResult.Data;
        Boleto boleto;

        if (fatura.Boleto == null)
        {
            var nossoNumero = string.IsNullOrWhiteSpace(data.LinhaDigitavel) ? $"NOSSO-{fatura.Id:D6}" : data.LinhaDigitavel;
            boleto = Boleto.Create(
                tenantId: fatura.TenantId,
                faturaId: fatura.Id,
                nossoNumero: nossoNumero,
                linhaDigitavel: data.LinhaDigitavel,
                codigoBarras: data.CodigoBarras,
                codigoPix: data.CodigoPixCopiaECola,
                valor: fatura.TotalFinal,
                dataVencimento: fatura.DataVencimento,
                pdfUrl: data.PdfUrl
            );

            fatura.AnexarBoleto(boleto);
            _dbContext.Boletos.Add(boleto);
        }
        else
        {
            boleto = fatura.Boleto;
        }

        boleto.VincularCobrancaGateway(
            externalChargeId: data.ExternalChargeId,
            provider: data.Provider,
            linhaDigitavel: data.LinhaDigitavel,
            codigoBarras: data.CodigoBarras,
            codigoPix: data.CodigoPixCopiaECola,
            qrCodeBase64: data.PixQrCodeBase64,
            pdfUrl: data.PdfUrl
        );

        await _dbContext.SaveChangesAsync(ct);

        return Result<PaymentInfoResponseDto>.Success(MapToPaymentInfo(fatura, boleto));
    }

    public async Task<Result<PaymentInfoResponseDto>> GetPaymentInfoAsync(int faturaId, CancellationToken ct = default)
    {
        if (faturaId <= 0)
        {
            return Result<PaymentInfoResponseDto>.ValidationFailure(new[] { "ID de fatura inválido." });
        }

        var fatura = await _dbContext.Faturas
            .Include(f => f.Boleto)
            .FirstOrDefaultAsync(f => f.Id == faturaId, ct);

        if (fatura == null)
        {
            return Result<PaymentInfoResponseDto>.Failure("Fatura não encontrada.");
        }

        if (fatura.Boleto == null)
        {
            // Se o boleto ainda não foi gerado, gera automaticamente sob demanda
            return await GeneratePaymentAsync(faturaId, ct);
        }

        return Result<PaymentInfoResponseDto>.Success(MapToPaymentInfo(fatura, fatura.Boleto));
    }

    public async Task<Result<PaymentInfoResponseDto>> SyncPaymentAsync(int faturaId, CancellationToken ct = default)
    {
        if (faturaId <= 0)
        {
            return Result<PaymentInfoResponseDto>.ValidationFailure(new[] { "ID de fatura inválido." });
        }

        var fatura = await _dbContext.Faturas
            .Include(f => f.Boleto)
            .FirstOrDefaultAsync(f => f.Id == faturaId, ct);

        if (fatura == null)
        {
            return Result<PaymentInfoResponseDto>.Failure("Fatura não encontrada.");
        }

        if (fatura.Boleto == null || string.IsNullOrWhiteSpace(fatura.Boleto.ExternalChargeId))
        {
            return await GeneratePaymentAsync(faturaId, ct);
        }

        var statusResult = await _gatewayService.ConsultarStatusCobrancaAsync(fatura.Boleto.ExternalChargeId, ct);
        if (statusResult.IsSuccess && statusResult.Data != null)
        {
            var statusData = statusResult.Data;
            if (statusData.Status == GatewayChargeStatus.Confirmed || statusData.Status == GatewayChargeStatus.Received)
            {
                var dtPagamento = statusData.DataPagamento ?? DateTime.UtcNow;
                var valPago = statusData.ValorPago ?? fatura.TotalFinal;
                fatura.Boleto.RegistrarPagamento(dtPagamento);
                fatura.RegistrarPagamento(dtPagamento, valPago);
                await _dbContext.SaveChangesAsync(ct);
            }
        }

        return Result<PaymentInfoResponseDto>.Success(MapToPaymentInfo(fatura, fatura.Boleto));
    }

    private static PaymentInfoResponseDto MapToPaymentInfo(Fatura fatura, Boleto boleto)
    {
        return new PaymentInfoResponseDto(
            FaturaId: fatura.Id,
            BoletoId: boleto.Id,
            ExternalChargeId: boleto.ExternalChargeId,
            Provider: boleto.GatewayProvider,
            LinhaDigitavel: boleto.LinhaDigitavel,
            CodigoBarras: boleto.CodigoBarras,
            CodigoPixCopiaECola: boleto.CodigoPixCopiaECola,
            PixQrCodeBase64: boleto.PixQrCodeBase64,
            PdfUrl: boleto.PdfUrl,
            StatusBoleto: boleto.Status.ToString(),
            StatusFatura: fatura.Status.ToString(),
            ValorOriginal: fatura.ValorOriginal,
            TotalFinal: fatura.TotalFinal,
            DataVencimento: fatura.DataVencimento,
            DataPagamento: fatura.DataPagamento,
            DataUltimaSincronizacao: boleto.DataUltimaSincronizacaoGateway
        );
    }
}
