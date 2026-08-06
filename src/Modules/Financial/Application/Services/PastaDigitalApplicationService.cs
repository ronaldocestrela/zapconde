using BuildingBlocks.Shared.MultiTenancy;
using BuildingBlocks.Shared;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;

namespace Modules.Financial.Application.Services;

public class PastaDigitalApplicationService : IPastaDigitalApplicationService
{
    private readonly FinancialDbContext _dbContext;
    private readonly ICurrentTenantService _currentTenantService;

    public PastaDigitalApplicationService(
        FinancialDbContext dbContext,
        ICurrentTenantService currentTenantService)
    {
        _dbContext = dbContext;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<PastaDigitalDto>> CriarPastaDigitalAsync(CriarPastaDigitalRequestDto request, CancellationToken ct = default)
    {
        int tenantId = _currentTenantService.TenantId ?? 1;

        var existente = await _dbContext.PastasDigitais
            .FirstOrDefaultAsync(p => p.CondoId == request.CondoId && p.Ano == request.Ano && p.Mes == request.Mes, ct);

        if (existente != null)
            return Result<PastaDigitalDto>.Failure($"Já existe uma pasta digital para o condomínio no período {request.Mes}/{request.Ano}.");

        var pasta = PastaDigital.Create(
            tenantId,
            request.CondoId,
            request.Ano,
            request.Mes,
            request.SaldoAnterior,
            request.ResumoIa);

        _dbContext.PastasDigitais.Add(pasta);
        await _dbContext.SaveChangesAsync(ct);

        return Result<PastaDigitalDto>.Success(MapToDto(pasta));
    }

    public async Task<Result<PastaDigitalDto>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var pasta = await _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pasta == null)
            return Result<PastaDigitalDto>.Failure($"Pasta digital com ID {id} não encontrada.");

        return Result<PastaDigitalDto>.Success(MapToDto(pasta));
    }

    public async Task<Result<IEnumerable<PastaDigitalDto>>> ListarPorCondominioAsync(int condoId, int? ano = null, CancellationToken ct = default)
    {
        var query = _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .Where(p => p.CondoId == condoId);

        if (ano.HasValue)
            query = query.Where(p => p.Ano == ano.Value);

        var list = await query.OrderByDescending(p => p.Ano).ThenByDescending(p => p.Mes).ToListAsync(ct);
        var dtos = list.Select(MapToDto);
        return Result<IEnumerable<PastaDigitalDto>>.Success(dtos);
    }

    public async Task<Result<PastaDigitalDto>> AdicionarItemBalanceteAsync(int pastaDigitalId, AdicionarItemBalanceteRequestDto request, CancellationToken ct = default)
    {
        var pasta = await _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .FirstOrDefaultAsync(p => p.Id == pastaDigitalId, ct);

        if (pasta == null)
            return Result<PastaDigitalDto>.Failure($"Pasta digital com ID {pastaDigitalId} não encontrada.");

        try
        {
            pasta.AdicionarItemBalancete(
                request.TipoLancamento,
                request.Categoria,
                request.Descricao,
                request.ValorOrcado,
                request.ValorRealizado,
                request.DataLancamento,
                request.Conciliado);

            await _dbContext.SaveChangesAsync(ct);
            return Result<PastaDigitalDto>.Success(MapToDto(pasta));
        }
        catch (Exception ex)
        {
            return Result<PastaDigitalDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PastaDigitalDto>> AnexarDocumentoAsync(int pastaDigitalId, AnexarDocumentoRequestDto request, CancellationToken ct = default)
    {
        var pasta = await _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .FirstOrDefaultAsync(p => p.Id == pastaDigitalId, ct);

        if (pasta == null)
            return Result<PastaDigitalDto>.Failure($"Pasta digital com ID {pastaDigitalId} não encontrada.");

        try
        {
            pasta.AnexarDocumento(
                request.Categoria,
                request.Titulo,
                request.NomeArquivo,
                request.UrlArquivo,
                request.ContentType,
                request.TamanhoBytes,
                request.UploadPorUserId);

            await _dbContext.SaveChangesAsync(ct);
            return Result<PastaDigitalDto>.Success(MapToDto(pasta));
        }
        catch (Exception ex)
        {
            return Result<PastaDigitalDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PastaDigitalDto>> SubmeterParaConselhoAsync(int pastaDigitalId, CancellationToken ct = default)
    {
        var pasta = await _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .FirstOrDefaultAsync(p => p.Id == pastaDigitalId, ct);

        if (pasta == null)
            return Result<PastaDigitalDto>.Failure($"Pasta digital com ID {pastaDigitalId} não encontrada.");

        try
        {
            pasta.SubmeterParaConselho();
            await _dbContext.SaveChangesAsync(ct);
            return Result<PastaDigitalDto>.Success(MapToDto(pasta));
        }
        catch (Exception ex)
        {
            return Result<PastaDigitalDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PastaDigitalDto>> AprovarPastaDigitalAsync(int pastaDigitalId, AprovarPastaDigitalRequestDto request, CancellationToken ct = default)
    {
        var pasta = await _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .FirstOrDefaultAsync(p => p.Id == pastaDigitalId, ct);

        if (pasta == null)
            return Result<PastaDigitalDto>.Failure($"Pasta digital com ID {pastaDigitalId} não encontrada.");

        try
        {
            pasta.Aprovar(request.AprovadoPorUserId, request.Parecer);
            await _dbContext.SaveChangesAsync(ct);
            return Result<PastaDigitalDto>.Success(MapToDto(pasta));
        }
        catch (Exception ex)
        {
            return Result<PastaDigitalDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PastaDigitalDto>> RejeitarPastaDigitalAsync(int pastaDigitalId, RejeitarPastaDigitalRequestDto request, CancellationToken ct = default)
    {
        var pasta = await _dbContext.PastasDigitais
            .Include(p => p.Documentos)
            .Include(p => p.ItensBalancete)
            .FirstOrDefaultAsync(p => p.Id == pastaDigitalId, ct);

        if (pasta == null)
            return Result<PastaDigitalDto>.Failure($"Pasta digital com ID {pastaDigitalId} não encontrada.");

        try
        {
            pasta.Rejeitar(request.ParecerMotivo);
            await _dbContext.SaveChangesAsync(ct);
            return Result<PastaDigitalDto>.Success(MapToDto(pasta));
        }
        catch (Exception ex)
        {
            return Result<PastaDigitalDto>.Failure(ex.Message);
        }
    }

    private static PastaDigitalDto MapToDto(PastaDigital pasta)
    {
        return new PastaDigitalDto(
            pasta.Id,
            pasta.TenantId,
            pasta.CondoId,
            pasta.Ano,
            pasta.Mes,
            pasta.Status,
            pasta.DataCriacao,
            pasta.DataFechamento,
            pasta.DataAprovacao,
            pasta.AprovadoPorUserId,
            pasta.ObservacoesConselho,
            pasta.ResumoExecutivoIa,
            pasta.SaldoAnterior,
            pasta.TotalReceitas,
            pasta.TotalDespesas,
            pasta.SaldoMes,
            pasta.SaldoAcumulado,
            pasta.Documentos.Select(d => new DocumentoPrestacaoDto(
                d.Id, d.PastaDigitalId, d.Categoria, d.Titulo, d.NomeArquivo, d.UrlArquivo, d.ContentType, d.TamanhoBytes, d.DataUpload)).ToList(),
            pasta.ItensBalancete.Select(i => new ItemBalanceteDto(
                i.Id, i.PastaDigitalId, i.TipoLancamento, i.Categoria, i.Descricao, i.ValorOrcado, i.ValorRealizado, i.DataLancamento, i.Conciliado)).ToList()
        );
    }
}
