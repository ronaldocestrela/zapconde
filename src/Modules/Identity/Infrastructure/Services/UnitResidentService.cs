using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application;
using Modules.Identity.Application.Dtos;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure.Persistence;

namespace Modules.Identity.Infrastructure.Services;

public sealed class UnitResidentService : IUnitResidentService
{
    private readonly IdentityDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UnitResidentService(IdentityDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<Result<IReadOnlyList<BlockDto>>> GetBlocksAsync(CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<IReadOnlyList<BlockDto>>.Failure(ctx.Message);
        }

        var blocks = await _db.Blocos
            .AsNoTracking()
            .OrderBy(b => b.Ordem)
            .ThenBy(b => b.Codigo)
            .Select(b => new BlockDto
            {
                Id = b.Id,
                Codigo = b.Codigo,
                Nome = b.Nome,
                Ordem = b.Ordem
            })
            .ToListAsync(ct);

        return Result<IReadOnlyList<BlockDto>>.Success(blocks);
    }

    public async Task<Result<BlockDto>> CreateBlockAsync(CreateBlockRequestDto request, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<BlockDto>.Failure(ctx.Message);
        }

        var (tenantId, condoId) = ctx.Data;

        if (await _db.Blocos.AnyAsync(b => b.Codigo == request.Codigo.Trim(), ct))
        {
            return Result<BlockDto>.Failure("Bloco já cadastrado neste condomínio.");
        }

        var bloco = Bloco.Create(tenantId, condoId, request.Codigo, request.Nome, request.Ordem);
        _db.Blocos.Add(bloco);
        await _db.SaveChangesAsync(ct);

        return Result<BlockDto>.Success(new BlockDto
        {
            Id = bloco.Id,
            Codigo = bloco.Codigo,
            Nome = bloco.Nome,
            Ordem = bloco.Ordem
        }, "Bloco criado com sucesso.");
    }

    public async Task<Result<IReadOnlyList<UnitListItemDto>>> ListUnitsAsync(UnitListQueryDto query, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<IReadOnlyList<UnitListItemDto>>.Failure(ctx.Message);
        }

        var unitsQuery = _db.Unidades
            .AsNoTracking()
            .Include(u => u.Bloco)
            .Include(u => u.Vinculos.Where(v => v.IsActive))
                .ThenInclude(v => v.Morador)
            .AsQueryable();

        if (query.BlockId.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.BlocoId == query.BlockId.Value);
        }

        if (query.Status.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.Status == query.Status.Value);
        }

        if (query.Papel.HasValue)
        {
            unitsQuery = unitsQuery.Where(u => u.Vinculos.Any(v => v.IsActive && v.Papel == query.Papel.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var term = query.Q.Trim().ToLowerInvariant();
            unitsQuery = unitsQuery.Where(u =>
                u.Numero.ToLower().Contains(term) ||
                u.Bloco!.Codigo.ToLower().Contains(term) ||
                u.Vinculos.Any(v => v.IsActive && v.Morador!.Nome.ToLower().Contains(term)));
        }

        var units = await unitsQuery
            .OrderBy(u => u.Bloco!.Ordem)
            .ThenBy(u => u.Bloco!.Codigo)
            .ThenBy(u => u.Numero)
            .ToListAsync(ct);

        var items = units.Select(MapListItem).ToList();
        return Result<IReadOnlyList<UnitListItemDto>>.Success(items);
    }

    public async Task<Result<UnitCreatedDto>> CreateUnitAsync(CreateUnitRequestDto request, string? userId, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<UnitCreatedDto>.Failure(ctx.Message);
        }

        var (tenantId, condoId) = ctx.Data;

        try
        {
            var blocoResult = await ResolveBlockAsync(tenantId, condoId, request.BlocoId, request.BlocoCodigo, ct);
            if (!blocoResult.IsSuccess)
            {
                return Result<UnitCreatedDto>.Failure(blocoResult.Message, blocoResult.Errors);
            }

            var bloco = blocoResult.Data!;

            if (await _db.Unidades.AnyAsync(u => u.BlocoId == bloco.Id && u.Numero == request.Numero.Trim(), ct))
            {
                return Result<UnitCreatedDto>.Failure("Unidade já cadastrada neste bloco.");
            }

            var morador = await FindOrCreateMoradorAsync(tenantId, condoId, request, ct);

            var unidade = Unidade.Create(tenantId, condoId, bloco.Id, request.Numero, request.Status);
            unidade.ValidarNovoVinculo(request.Papel);

            _db.Unidades.Add(unidade);
            await _db.SaveChangesAsync(ct);

            var vinculo = VinculoUnidade.Create(
                tenantId, condoId, unidade.Id, morador.Id, request.Papel,
                request.DataInicio, request.Dependencias, userId);

            _db.VinculosUnidade.Add(vinculo);

            if (request.Status != UnidadeStatus.EmReforma)
            {
                unidade.RecalcularStatus();
            }

            await _db.SaveChangesAsync(ct);

            return Result<UnitCreatedDto>.Success(new UnitCreatedDto
            {
                UnitId = unidade.Id,
                ResidentId = morador.Id,
                VinculoId = vinculo.Id
            }, "Unidade e morador cadastrados com sucesso.");
        }
        catch (DomainValidationException ex)
        {
            return Result<UnitCreatedDto>.ValidationFailure(ex.Message, [ex.Message]);
        }
    }

    public async Task<Result<UnitListItemDto>> UpdateUnitAsync(int unitId, UpdateUnitRequestDto request, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<UnitListItemDto>.Failure(ctx.Message);
        }

        var unidade = await _db.Unidades
            .Include(u => u.Bloco)
            .Include(u => u.Vinculos.Where(v => v.IsActive))
                .ThenInclude(v => v.Morador)
            .FirstOrDefaultAsync(u => u.Id == unitId, ct);

        if (unidade is null)
        {
            return Result<UnitListItemDto>.Failure("Unidade não encontrada.");
        }

        try
        {
            unidade.AtualizarStatus(request.Status);

            var vinculoAtivo = unidade.Vinculos.FirstOrDefault(v => v.IsActive);
            if (vinculoAtivo?.Morador is not null)
            {
                vinculoAtivo.Morador.Atualizar(
                    request.MoradorNome, request.MoradorCpf,
                    request.MoradorEmail, request.MoradorTelefone);
                vinculoAtivo.AtualizarDependencias(request.Dependencias);
            }

            if (request.Status != UnidadeStatus.EmReforma)
            {
                unidade.RecalcularStatus();
            }

            await _db.SaveChangesAsync(ct);
            return Result<UnitListItemDto>.Success(MapListItem(unidade));
        }
        catch (DomainValidationException ex)
        {
            return Result<UnitListItemDto>.ValidationFailure(ex.Message, [ex.Message]);
        }
    }

    public async Task<Result> TransferOwnershipAsync(int unitId, TransferOwnershipRequestDto request, string? userId, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result.Failure(ctx.Message);
        }

        var (tenantId, condoId) = ctx.Data;

        var unidade = await _db.Unidades
            .Include(u => u.Vinculos.Where(v => v.IsActive))
                .ThenInclude(v => v.Morador)
            .FirstOrDefaultAsync(u => u.Id == unitId, ct);

        if (unidade is null)
        {
            return Result.Failure("Unidade não encontrada.");
        }

        try
        {
            var vinculoAtivo = unidade.ObterVinculoAtivo(request.Papel);
            if (vinculoAtivo is not null)
            {
                vinculoAtivo.Encerrar(request.DataEncerramento, request.Motivo);
            }

            var morador = await FindOrCreateMoradorAsync(tenantId, condoId, new CreateUnitRequestDto
            {
                MoradorNome = request.NovoMoradorNome,
                MoradorCpf = request.NovoMoradorCpf,
                MoradorEmail = request.NovoMoradorEmail,
                MoradorTelefone = request.NovoMoradorTelefone
            }, ct);

            unidade.ValidarNovoVinculo(request.Papel);

            var novoVinculo = VinculoUnidade.Create(
                tenantId, condoId, unidade.Id, morador.Id, request.Papel,
                request.DataInicio, request.Dependencias, userId);

            _db.VinculosUnidade.Add(novoVinculo);
            unidade.RecalcularStatus();
            await _db.SaveChangesAsync(ct);

            return Result.Success("Titularidade transferida com sucesso. Vínculo anterior arquivado no histórico.");
        }
        catch (DomainValidationException ex)
        {
            return Result.ValidationFailure(ex.Message, [ex.Message]);
        }
    }

    public async Task<Result<IReadOnlyList<UnitHistoryItemDto>>> GetHistoryAsync(int unitId, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<IReadOnlyList<UnitHistoryItemDto>>.Failure(ctx.Message);
        }

        var exists = await _db.Unidades.AnyAsync(u => u.Id == unitId, ct);
        if (!exists)
        {
            return Result<IReadOnlyList<UnitHistoryItemDto>>.Failure("Unidade não encontrada.");
        }

        var history = await _db.VinculosUnidade
            .AsNoTracking()
            .Include(v => v.Morador)
            .Where(v => v.UnidadeId == unitId)
            .OrderByDescending(v => v.DataInicio)
            .Select(v => new UnitHistoryItemDto
            {
                VinculoId = v.Id,
                MoradorNome = v.Morador!.Nome,
                Papel = v.Papel,
                DataInicio = v.DataInicio,
                DataFim = v.DataFim,
                MotivoEncerramento = v.MotivoEncerramento,
                IsActive = v.IsActive,
                CreatedByUserId = v.CreatedByUserId,
                Dependencias = v.Dependencias
            })
            .ToListAsync(ct);

        return Result<IReadOnlyList<UnitHistoryItemDto>>.Success(history);
    }

    public Task<byte[]> GetImportTemplateAsync(CancellationToken ct = default)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Unidades");
        var headers = new[] { "Bloco", "Unidade", "Morador", "CPF", "E-mail", "Telefone/WhatsApp", "Papel" };
        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        sheet.Cell(2, 1).Value = "Bloco A";
        sheet.Cell(2, 2).Value = "101";
        sheet.Cell(2, 3).Value = "João Silva";
        sheet.Cell(2, 4).Value = "529.982.247-25";
        sheet.Cell(2, 5).Value = "joao@email.com";
        sheet.Cell(2, 6).Value = "+5511999999999";
        sheet.Cell(2, 7).Value = "Proprietario";

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public async Task<Result<ImportPreviewResultDto>> PreviewImportAsync(Stream fileStream, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<ImportPreviewResultDto>.Failure(ctx.Message);
        }

        var rows = ParseImportRows(fileStream);
        var result = new ImportPreviewResultDto
        {
            TotalRows = rows.Count,
            Rows = rows
        };

        result.ValidRows = rows.Count(r => r.IsValid);
        result.InvalidRows = rows.Count(r => !r.IsValid);

        return Result<ImportPreviewResultDto>.Success(result);
    }

    public async Task<Result<ImportCommitResultDto>> CommitImportAsync(ImportCommitRequestDto request, string? userId, CancellationToken ct = default)
    {
        var ctx = RequireContext();
        if (!ctx.IsSuccess)
        {
            return Result<ImportCommitResultDto>.Failure(ctx.Message);
        }

        var (tenantId, condoId) = ctx.Data;
        var imported = 0;
        var skipped = 0;

        foreach (var row in request.Rows.Where(r => r.IsValid))
        {
            var createResult = await CreateUnitAsync(new CreateUnitRequestDto
            {
                BlocoCodigo = row.BlocoCodigo,
                Numero = row.Numero,
                MoradorNome = row.MoradorNome,
                MoradorCpf = row.MoradorCpf,
                MoradorEmail = row.MoradorEmail,
                MoradorTelefone = row.MoradorTelefone,
                Papel = row.Papel
            }, userId, ct);

            if (createResult.IsSuccess)
            {
                imported++;
            }
            else
            {
                skipped++;
            }
        }

        return Result<ImportCommitResultDto>.Success(new ImportCommitResultDto
        {
            ImportedCount = imported,
            SkippedCount = skipped
        }, $"{imported} unidade(s) importada(s) com sucesso.");
    }

    private Result<(int TenantId, int CondoId)> RequireContext()
    {
        if (!_tenant.TenantId.HasValue || !_tenant.CondoId.HasValue)
        {
            return Result<(int, int)>.Failure("Contexto de tenant/condomínio não resolvido.");
        }

        return Result<(int, int)>.Success((_tenant.TenantId.Value, _tenant.CondoId.Value));
    }

    private async Task<Result<Bloco>> ResolveBlockAsync(int tenantId, int condoId, int? blocoId, string? blocoCodigo, CancellationToken ct)
    {
        Bloco? bloco = null;

        if (blocoId.HasValue)
        {
            bloco = await _db.Blocos.FirstOrDefaultAsync(b => b.Id == blocoId.Value, ct);
        }
        else if (!string.IsNullOrWhiteSpace(blocoCodigo))
        {
            bloco = await _db.Blocos.FirstOrDefaultAsync(b => b.Codigo == blocoCodigo.Trim(), ct);
            if (bloco is null)
            {
                bloco = Bloco.Create(tenantId, condoId, blocoCodigo.Trim());
                _db.Blocos.Add(bloco);
                await _db.SaveChangesAsync(ct);
            }
        }

        return bloco is null
            ? Result<Bloco>.ValidationFailure("Bloco é obrigatório.", ["Informe blocoId ou blocoCodigo."])
            : Result<Bloco>.Success(bloco);
    }

    private async Task<Morador> FindOrCreateMoradorAsync(int tenantId, int condoId, CreateUnitRequestDto request, CancellationToken ct)
    {
        var cpf = CpfValidator.Normalize(request.MoradorCpf);
        var existing = await _db.Moradores.FirstOrDefaultAsync(m => m.Cpf == cpf, ct);

        if (existing is not null)
        {
            existing.Atualizar(request.MoradorNome, request.MoradorCpf, request.MoradorEmail, request.MoradorTelefone);
            return existing;
        }

        var morador = Morador.Create(
            tenantId, condoId,
            request.MoradorNome, request.MoradorCpf,
            request.MoradorEmail, request.MoradorTelefone);

        _db.Moradores.Add(morador);
        await _db.SaveChangesAsync(ct);
        return morador;
    }

    private static UnitListItemDto MapListItem(Unidade u)
    {
        var vinculo = u.Vinculos.FirstOrDefault(v => v.IsActive);
        return new UnitListItemDto
        {
            UnitId = u.Id,
            BlocoId = u.BlocoId,
            BlocoCodigo = u.Bloco?.Codigo ?? string.Empty,
            Numero = u.Numero,
            Status = u.Status,
            MoradorNome = vinculo?.Morador?.Nome,
            Papel = vinculo?.Papel,
            MoradorTelefone = vinculo?.Morador?.TelefoneWhatsApp,
            PhoneVerificationStatus = vinculo?.Morador?.PhoneVerificationStatus
                ?? PhoneVerificationStatus.NaoInformado,
            DataInicio = vinculo?.DataInicio,
            MoradorId = vinculo?.MoradorId,
            VinculoId = vinculo?.Id
        };
    }

    private static List<ImportPreviewRowDto> ParseImportRows(Stream fileStream)
    {
        using var workbook = new XLWorkbook(fileStream);
        var sheet = workbook.Worksheet(1);
        var rows = new List<ImportPreviewRowDto>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var r = 2; r <= lastRow; r++)
        {
            var bloco = sheet.Cell(r, 1).GetString().Trim();
            var numero = sheet.Cell(r, 2).GetString().Trim();
            if (string.IsNullOrWhiteSpace(bloco) && string.IsNullOrWhiteSpace(numero))
            {
                continue;
            }

            var row = new ImportPreviewRowDto
            {
                RowNumber = r,
                BlocoCodigo = bloco,
                Numero = numero,
                MoradorNome = sheet.Cell(r, 3).GetString().Trim(),
                MoradorCpf = sheet.Cell(r, 4).GetString().Trim(),
                MoradorEmail = sheet.Cell(r, 5).GetString().Trim(),
                MoradorTelefone = sheet.Cell(r, 6).GetString().Trim(),
                Papel = ParsePapel(sheet.Cell(r, 7).GetString())
            };

            ValidateImportRow(row);
            rows.Add(row);
        }

        return rows;
    }

    private static PapelVinculo ParsePapel(string value) =>
        value.Trim().Equals("Inquilino", StringComparison.OrdinalIgnoreCase)
            ? PapelVinculo.Inquilino
            : PapelVinculo.Proprietario;

    private static void ValidateImportRow(ImportPreviewRowDto row)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(row.BlocoCodigo))
        {
            errors.Add("Bloco é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(row.Numero))
        {
            errors.Add("Unidade é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(row.MoradorNome))
        {
            errors.Add("Nome do morador é obrigatório.");
        }

        if (!CpfValidator.IsValid(row.MoradorCpf))
        {
            errors.Add("CPF inválido.");
        }

        if (string.IsNullOrWhiteSpace(row.MoradorEmail) || !row.MoradorEmail.Contains('@'))
        {
            errors.Add("E-mail inválido.");
        }

        row.Errors = errors;
        row.IsValid = errors.Count == 0;
    }
}
