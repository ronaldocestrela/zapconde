using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public InvoiceService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<IEnumerable<FaturaSummaryDto>>> GetInvoicesAsync(
        int? condoId = null,
        int? unidadeId = null,
        string? competencia = null,
        StatusFatura? status = null,
        CancellationToken ct = default)
    {
        if (!_currentTenantService.TenantId.HasValue)
        {
            return Result<IEnumerable<FaturaSummaryDto>>.Failure("Tenant não resolvido no contexto atual.");
        }

        var query = _dbContext.Faturas
            .Include(f => f.Boleto)
            .AsNoTracking()
            .AsQueryable();

        if (condoId.HasValue && condoId.Value > 0)
        {
            query = query.Where(f => f.CondoId == condoId.Value);
        }

        if (unidadeId.HasValue && unidadeId.Value > 0)
        {
            query = query.Where(f => f.UnidadeId == unidadeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(competencia))
        {
            query = query.Where(f => f.Competencia == competencia.Trim());
        }

        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        var faturas = await query
            .OrderByDescending(f => f.DataVencimento)
            .ToListAsync(ct);

        var dtos = faturas.Select(MapToSummary);

        return Result<IEnumerable<FaturaSummaryDto>>.Success(dtos);
    }

    public async Task<Result<FaturaDetailDto>> GetInvoiceByIdAsync(int id, CancellationToken ct = default)
    {
        if (!_currentTenantService.TenantId.HasValue)
        {
            return Result<FaturaDetailDto>.Failure("Tenant não resolvido no contexto atual.");
        }

        var fatura = await _dbContext.Faturas
            .Include(f => f.Itens)
            .Include(f => f.Boleto)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fatura == null)
        {
            return Result<FaturaDetailDto>.Failure($"Fatura com ID {id} não foi encontrada.");
        }

        return Result<FaturaDetailDto>.Success(MapToDetail(fatura));
    }

    public async Task<Result<FaturaDetailDto>> CreateInvoiceAsync(CreateFaturaRequest request, CancellationToken ct = default)
    {
        if (!_currentTenantService.TenantId.HasValue)
        {
            return Result<FaturaDetailDto>.Failure("Tenant não resolvido no contexto atual.");
        }

        var tenantId = _currentTenantService.TenantId.Value;

        if (request.UnidadeId <= 0)
            return Result<FaturaDetailDto>.ValidationFailure(new[] { "UnidadeId é obrigatório." });

        if (request.MoradorId <= 0)
            return Result<FaturaDetailDto>.ValidationFailure(new[] { "MoradorId é obrigatório." });

        if (string.IsNullOrWhiteSpace(request.Competencia))
            return Result<FaturaDetailDto>.ValidationFailure(new[] { "Competência é obrigatória." });

        if (request.Itens == null || !request.Itens.Any())
            return Result<FaturaDetailDto>.ValidationFailure(new[] { "A fatura deve conter pelo menos 1 item de cobrança." });

        var fatura = Fatura.Create(
            tenantId: tenantId,
            condoId: request.CondoId > 0 ? request.CondoId : (_currentTenantService.CondoId ?? 1),
            unidadeId: request.UnidadeId,
            moradorId: request.MoradorId,
            competencia: request.Competencia,
            dataVencimento: request.DataVencimento,
            observacoes: request.Observacoes ?? string.Empty
        );

        foreach (var item in request.Itens)
        {
            fatura.AddItem(item.Descricao, item.Tipo, item.ValorUnitario, item.Quantidade);
        }

        // Criar BoletoStub automático
        var nossoNumero = $"34190{Random.Shared.Next(100000, 999999)}";
        var linhaDigitavel = $"34191.79001 {Random.Shared.Next(10000, 99999)}.000005 00000.000002 8 {DateTime.Today:yyMMdd}0000";
        var codigoBarras = $"34198{DateTime.Today:yyMMdd}{((int)(fatura.TotalFinal * 100)):D10}0000000000";
        var codigoPix = $"00020126580014br.gov.bcb.pix0136zapcondo-pix-{tenantId}-{fatura.NumeroFatura.ToLower()}5204000053039865405{fatura.TotalFinal:F2}5802BR5915SmartCondo SaaS6009SAO PAULO62070503***6304ABCD";

        var boleto = Boleto.Create(
            tenantId: tenantId,
            faturaId: fatura.Id,
            nossoNumero: nossoNumero,
            linhaDigitavel: linhaDigitavel,
            codigoBarras: codigoBarras,
            codigoPix: codigoPix,
            valor: fatura.TotalFinal,
            dataVencimento: fatura.DataVencimento,
            pdfUrl: $"/api/financial/invoices/{fatura.Id}/pdf"
        );

        fatura.AnexarBoleto(boleto);

        _dbContext.Faturas.Add(fatura);
        await _dbContext.SaveChangesAsync(ct);

        return Result<FaturaDetailDto>.Success(MapToDetail(fatura));
    }

    public async Task<Result> CancelInvoiceAsync(int id, CancellationToken ct = default)
    {
        if (!_currentTenantService.TenantId.HasValue)
        {
            return Result.Failure("Tenant não resolvido no contexto atual.");
        }

        var fatura = await _dbContext.Faturas
            .Include(f => f.Boleto)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (fatura == null)
        {
            return Result.Failure($"Fatura com ID {id} não foi encontrada.");
        }

        try
        {
            fatura.Cancelar();
            await _dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Não foi possível cancelar a fatura: {ex.Message}");
        }
    }

    private static FaturaSummaryDto MapToSummary(Fatura f) => new(
        Id: f.Id,
        CondoId: f.CondoId,
        UnidadeId: f.UnidadeId,
        BlocoNumeroUnidade: $"Unidade {f.UnidadeId}",
        MoradorId: f.MoradorId,
        NomeMorador: $"Morador #{f.MoradorId}",
        Competencia: f.Competencia,
        NumeroFatura: f.NumeroFatura,
        DataEmissao: f.DataEmissao,
        DataVencimento: f.DataVencimento,
        ValorOriginal: f.ValorOriginal,
        ValorDesconto: f.ValorDesconto,
        ValorMulta: f.ValorMulta,
        ValorJuros: f.ValorJuros,
        TotalFinal: f.TotalFinal,
        Status: f.Status,
        StatusDescricao: f.Status.ToString(),
        DataPagamento: f.DataPagamento,
        TemBoleto: f.Boleto != null
    );

    private static FaturaDetailDto MapToDetail(Fatura f) => new(
        Id: f.Id,
        CondoId: f.CondoId,
        UnidadeId: f.UnidadeId,
        BlocoNumeroUnidade: $"Unidade {f.UnidadeId}",
        MoradorId: f.MoradorId,
        NomeMorador: $"Morador #{f.MoradorId}",
        Competencia: f.Competencia,
        NumeroFatura: f.NumeroFatura,
        DataEmissao: f.DataEmissao,
        DataVencimento: f.DataVencimento,
        ValorOriginal: f.ValorOriginal,
        ValorDesconto: f.ValorDesconto,
        ValorMulta: f.ValorMulta,
        ValorJuros: f.ValorJuros,
        TotalFinal: f.TotalFinal,
        Status: f.Status,
        StatusDescricao: f.Status.ToString(),
        DataPagamento: f.DataPagamento,
        Observacoes: f.Observacoes,
        Itens: f.Itens.Select(i => new ItemCobrancaDto(
            Id: i.Id,
            Descricao: i.Descricao,
            Tipo: i.Tipo,
            TipoDescricao: i.Tipo.ToString(),
            ValorUnitario: i.ValorUnitario,
            Quantidade: i.Quantidade,
            Subtotal: i.Subtotal
        )),
        Boleto: f.Boleto != null ? new BoletoDto(
            Id: f.Boleto.Id,
            NossoNumero: f.Boleto.NossoNumero,
            LinhaDigitavel: f.Boleto.LinhaDigitavel,
            CodigoBarras: f.Boleto.CodigoBarras,
            CodigoPixCopiaECola: f.Boleto.CodigoPixCopiaECola,
            QrCodeUrl: f.Boleto.QrCodeUrl,
            PdfUrl: f.Boleto.PdfUrl,
            Valor: f.Boleto.Valor,
            DataVencimento: f.Boleto.DataVencimento,
            DataEmissao: f.Boleto.DataEmissao,
            DataPagamento: f.Boleto.DataPagamento,
            Status: f.Boleto.Status,
            StatusDescricao: f.Boleto.Status.ToString()
        ) : null
    );
}
