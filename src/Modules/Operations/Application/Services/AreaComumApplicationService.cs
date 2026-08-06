using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Repositories;

namespace Modules.Operations.Application.Services;

public class AreaComumApplicationService : IAreaComumApplicationService
{
    private readonly IAreaComumRepository _repository;
    private readonly ICurrentTenantService _currentTenantService;

    public AreaComumApplicationService(
        IAreaComumRepository repository,
        ICurrentTenantService currentTenantService)
    {
        _repository = repository;
        _currentTenantService = currentTenantService;
    }

    public async Task<Result<AreaComumDto>> CreateAsync(CreateAreaComumRequest request, CancellationToken ct = default)
    {
        try
        {
            var tenantId = _currentTenantService.TenantId;
            if (!tenantId.HasValue || tenantId.Value <= 0)
                return Result<AreaComumDto>.Failure("Tenant não identificado no contexto da requisição.");

            if (request.CondoId <= 0)
                return Result<AreaComumDto>.ValidationFailure(new[] { "CondoId é obrigatório." });

            var exists = await _repository.ExistsByNameAsync(request.CondoId, request.Nome, ct: ct);
            if (exists)
                return Result<AreaComumDto>.Failure($"Já existe uma área comum cadastrada com o nome '{request.Nome}'.");

            if (!TimeSpan.TryParse(request.HorarioInicioFuncionamento, out var horarioInicio))
                return Result<AreaComumDto>.ValidationFailure(new[] { "Horário de início inválido. Use o formato HH:mm." });

            if (!TimeSpan.TryParse(request.HorarioFimFuncionamento, out var horarioFim))
                return Result<AreaComumDto>.ValidationFailure(new[] { "Horário de término inválido. Use o formato HH:mm." });

            var areaComum = AreaComum.Create(
                tenantId.Value,
                request.CondoId,
                request.Nome,
                request.Descricao,
                request.Tipo,
                request.CapacidadeMaxima,
                request.TaxaReserva,
                request.TaxaLimpeza,
                horarioInicio,
                horarioFim,
                request.TempoAntecedenciaMinimaDias,
                request.TempoAntecedenciaMaximaDias,
                request.RequerAprovacaoSindico,
                request.RegrasUso);

            await _repository.AddAsync(areaComum, ct);
            await _repository.SaveChangesAsync(ct);

            return Result<AreaComumDto>.Success(MapToDto(areaComum), "Área comum cadastrada com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return Result<AreaComumDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AreaComumDto>.Failure($"Erro ao cadastrar área comum: {ex.Message}");
        }
    }

    public async Task<Result<AreaComumDto>> UpdateAsync(int id, UpdateAreaComumRequest request, CancellationToken ct = default)
    {
        try
        {
            var area = await _repository.GetByIdAsync(id, ct);
            if (area == null)
                return Result<AreaComumDto>.Failure($"Área comum com ID {id} não foi encontrada.");

            var exists = await _repository.ExistsByNameAsync(area.CondoId, request.Nome, ignoreId: id, ct: ct);
            if (exists)
                return Result<AreaComumDto>.Failure($"Já existe outra área comum com o nome '{request.Nome}'.");

            if (!TimeSpan.TryParse(request.HorarioInicioFuncionamento, out var horarioInicio))
                return Result<AreaComumDto>.ValidationFailure(new[] { "Horário de início inválido. Use o formato HH:mm." });

            if (!TimeSpan.TryParse(request.HorarioFimFuncionamento, out var horarioFim))
                return Result<AreaComumDto>.ValidationFailure(new[] { "Horário de término inválido. Use o formato HH:mm." });

            area.AtualizarDados(
                request.Nome,
                request.Descricao,
                request.Tipo,
                request.CapacidadeMaxima,
                horarioInicio,
                horarioFim,
                request.RequerAprovacaoSindico,
                request.RegrasUso);

            area.AtualizarRegrasECustos(
                request.TaxaReserva,
                request.TaxaLimpeza,
                request.TempoAntecedenciaMinimaDias,
                request.TempoAntecedenciaMaximaDias);

            await _repository.UpdateAsync(area, ct);
            await _repository.SaveChangesAsync(ct);

            return Result<AreaComumDto>.Success(MapToDto(area), "Área comum atualizada com sucesso.");
        }
        catch (ArgumentException ex)
        {
            return Result<AreaComumDto>.ValidationFailure(new[] { ex.Message });
        }
        catch (Exception ex)
        {
            return Result<AreaComumDto>.Failure($"Erro ao atualizar área comum: {ex.Message}");
        }
    }

    public async Task<Result<AreaComumDto>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var area = await _repository.GetByIdAsync(id, ct);
        if (area == null)
            return Result<AreaComumDto>.Failure($"Área comum com ID {id} não encontrada.");

        return Result<AreaComumDto>.Success(MapToDto(area));
    }

    public async Task<Result<IEnumerable<AreaComumDto>>> GetAllAsync(
        int condoId,
        StatusAreaComum? status = null,
        TipoAreaComum? tipo = null,
        CancellationToken ct = default)
    {
        var list = await _repository.GetAllAsync(condoId, status, tipo, ct);
        var dtos = list.Select(MapToDto);
        return Result<IEnumerable<AreaComumDto>>.Success(dtos);
    }

    public async Task<Result<AreaComumDto>> ChangeStatusAsync(int id, ChangeAreaComumStatusRequest request, CancellationToken ct = default)
    {
        var area = await _repository.GetByIdAsync(id, ct);
        if (area == null)
            return Result<AreaComumDto>.Failure($"Área comum com ID {id} não encontrada.");

        area.AlterarStatus(request.NovoStatus);
        await _repository.UpdateAsync(area, ct);
        await _repository.SaveChangesAsync(ct);

        return Result<AreaComumDto>.Success(MapToDto(area), $"Status alterado para {request.NovoStatus} com sucesso.");
    }

    public async Task<Result<AreaComumSummaryDto>> GetSummaryAsync(int condoId, CancellationToken ct = default)
    {
        var list = (await _repository.GetAllAsync(condoId, ct: ct)).ToList();

        var total = list.Count;
        var ativas = list.Count(x => x.Status == StatusAreaComum.Ativa);
        var manutencao = list.Count(x => x.Status == StatusAreaComum.Manutencao);
        var inativas = list.Count(x => x.Status == StatusAreaComum.Inativa);

        var mediaReserva = total > 0 ? list.Average(x => x.TaxaReserva) : 0m;
        var mediaLimpeza = total > 0 ? list.Average(x => x.TaxaLimpeza) : 0m;

        var summary = new AreaComumSummaryDto(
            total,
            ativas,
            manutencao,
            inativas,
            Math.Round(mediaReserva, 2),
            Math.Round(mediaLimpeza, 2));

        return Result<AreaComumSummaryDto>.Success(summary);
    }

    private static AreaComumDto MapToDto(AreaComum area)
    {
        return new AreaComumDto(
            area.Id,
            area.TenantId,
            area.CondoId,
            area.Nome,
            area.Descricao,
            area.Tipo,
            area.Tipo.ToString(),
            area.Status,
            area.Status.ToString(),
            area.CapacidadeMaxima,
            area.TaxaReserva,
            area.TaxaLimpeza,
            area.CustoTotalReserva,
            area.HorarioInicioFuncionamento.ToString(@"hh\:mm"),
            area.HorarioFimFuncionamento.ToString(@"hh\:mm"),
            area.TempoAntecedenciaMinimaDias,
            area.TempoAntecedenciaMaximaDias,
            area.RequerAprovacaoSindico,
            area.RegrasUso,
            area.DataCriacao,
            area.DataAtualizacao);
    }
}
