using BuildingBlocks.Shared;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Repositories;

namespace Modules.Operations.Application.Services;

public class ReservaApplicationService : IReservaApplicationService
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IAreaComumRepository _areaComumRepository;
    private readonly IDistributedLockService _lockService;
    private readonly ICurrentTenantService _currentTenantService;

    public ReservaApplicationService(
        IReservaRepository reservaRepository,
        IAreaComumRepository areaComumRepository,
        IDistributedLockService lockService,
        ICurrentTenantService currentTenantService)
    {
        _reservaRepository = reservaRepository ?? throw new ArgumentNullException(nameof(reservaRepository));
        _areaComumRepository = areaComumRepository ?? throw new ArgumentNullException(nameof(areaComumRepository));
        _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
        _currentTenantService = currentTenantService ?? throw new ArgumentNullException(nameof(currentTenantService));
    }

    public async Task<Result<ReservaDto>> CriarReservaAsync(CreateReservaRequest request, CancellationToken ct = default)
    {
        try
        {
            var tenantId = _currentTenantService.TenantId;
            if (!tenantId.HasValue || tenantId.Value <= 0)
                return Result<ReservaDto>.Failure("Tenant não identificado no contexto da requisição.");

            if (request.CondoId <= 0)
                return Result<ReservaDto>.ValidationFailure(new[] { "CondoId é obrigatório." });

            if (request.AreaComumId <= 0)
                return Result<ReservaDto>.ValidationFailure(new[] { "AreaComumId é obrigatório." });

            if (request.MoradorId <= 0)
                return Result<ReservaDto>.ValidationFailure(new[] { "MoradorId é obrigatório." });

            if (request.DataInicio >= request.DataFim)
                return Result<ReservaDto>.ValidationFailure(new[] { "A data/hora de início deve ser anterior à data/hora de término." });

            var areaComum = await _areaComumRepository.GetByIdAsync(request.AreaComumId, ct);
            if (areaComum == null)
                return Result<ReservaDto>.Failure("Área comum não encontrada.");

            if (areaComum.Status != StatusAreaComum.Ativa)
                return Result<ReservaDto>.Failure($"A área comum '{areaComum.Nome}' não está ativa para reservas.");

            var dataInicioUtc = request.DataInicio.Kind == DateTimeKind.Utc ? request.DataInicio : DateTime.SpecifyKind(request.DataInicio, DateTimeKind.Utc);
            var dataFimUtc = request.DataFim.Kind == DateTimeKind.Utc ? request.DataFim : DateTime.SpecifyKind(request.DataFim, DateTimeKind.Utc);

            // Redis Distributed Lock para garantir atomicidade sob requisições simultâneas
            var lockKey = $"lock:operations:tenant:{tenantId.Value}:areacomum:{request.AreaComumId}:date:{dataInicioUtc:yyyyMMdd}";
            await using var lockHandle = await _lockService.AcquireLockAsync(
                lockKey,
                expiry: TimeSpan.FromSeconds(10),
                timeout: TimeSpan.FromSeconds(3),
                cancellationToken: ct);

            if (!lockHandle.IsAcquired)
            {
                return Result<ReservaDto>.Failure("Ocorreu concorrência no agendamento desta área comum. Por favor, tente novamente.");
            }

            // Checagem de sobreposição temporal dentro da seção crítica com lock distribuído
            var hasOverlap = await _reservaRepository.HasOverlappingReservationAsync(
                request.CondoId,
                request.AreaComumId,
                dataInicioUtc,
                dataFimUtc,
                ignoreReservaId: null,
                ct: ct);

            if (hasOverlap)
            {
                return Result<ReservaDto>.Failure("Já existe uma reserva confirmada ou pendente para esta área comum no horário solicitado.");
            }

            Reserva reserva;
            try
            {
                reserva = Reserva.Create(
                    tenantId.Value,
                    request.CondoId,
                    areaComum,
                    request.MoradorId,
                    request.NomeMorador,
                    request.UnidadeMorador,
                    dataInicioUtc,
                    dataFimUtc,
                    request.QuantidadePessoas,
                    request.Observacao);
            }
            catch (ArgumentException ex)
            {
                return Result<ReservaDto>.ValidationFailure(new[] { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Result<ReservaDto>.Failure(ex.Message);
            }

            await _reservaRepository.AddAsync(reserva, ct);

            return Result<ReservaDto>.Success(MapToDto(reserva), "Reserva realizada com sucesso.");
        }
        catch (Exception ex)
        {
            return Result<ReservaDto>.Failure($"Erro ao criar reserva: {ex.Message}");
        }
    }

    public async Task<Result<ReservaDto>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id, ct);
        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva não encontrada.");

        return Result<ReservaDto>.Success(MapToDto(reserva));
    }

    public async Task<Result<IEnumerable<ReservaDto>>> ListarReservasAsync(
        int condoId,
        int? areaComumId = null,
        int? moradorId = null,
        StatusReserva? status = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken ct = default)
    {
        var reservas = await _reservaRepository.GetAllAsync(condoId, areaComumId, moradorId, status, dataInicio, dataFim, ct);
        var dtos = reservas.Select(MapToDto);
        return Result<IEnumerable<ReservaDto>>.Success(dtos);
    }

    public async Task<Result<ReservaDto>> CancelarReservaAsync(int id, CancelarReservaRequest request, CancellationToken ct = default)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id, ct);
        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva não encontrada.");

        try
        {
            reserva.Cancelar(request.Motivo);
            await _reservaRepository.UpdateAsync(reserva, ct);
            return Result<ReservaDto>.Success(MapToDto(reserva), "Reserva cancelada com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return Result<ReservaDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ReservaDto>> AprovarReservaAsync(int id, CancellationToken ct = default)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id, ct);
        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva não encontrada.");

        try
        {
            reserva.Aprovar();
            await _reservaRepository.UpdateAsync(reserva, ct);
            return Result<ReservaDto>.Success(MapToDto(reserva), "Reserva aprovada com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return Result<ReservaDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ReservaDto>> RejeitarReservaAsync(int id, RejeitarReservaRequest request, CancellationToken ct = default)
    {
        var reserva = await _reservaRepository.GetByIdAsync(id, ct);
        if (reserva == null)
            return Result<ReservaDto>.Failure("Reserva não encontrada.");

        try
        {
            reserva.Rejeitar(request.Motivo);
            await _reservaRepository.UpdateAsync(reserva, ct);
            return Result<ReservaDto>.Success(MapToDto(reserva), "Reserva rejeitada com sucesso.");
        }
        catch (InvalidOperationException ex)
        {
            return Result<ReservaDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ReservaSummaryDto>> ObterResumoAsync(int condoId, CancellationToken ct = default)
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1).AddTicks(-1);

        var reservasMes = await _reservaRepository.GetAllAsync(condoId, dataInicio: inicioMes, dataFim: fimMes, ct: ct);
        var list = reservasMes.ToList();

        var totalMes = list.Count;
        var pendentes = list.Count(x => x.Status == StatusReserva.PendenteAprovacao);
        var confirmadas = list.Count(x => x.Status == StatusReserva.Confirmada);
        var receitaTotal = list.Where(x => x.Status == StatusReserva.Confirmada).Sum(x => x.ValorTotal);

        var summary = new ReservaSummaryDto(totalMes, pendentes, confirmadas, Math.Round(receitaTotal, 2));
        return Result<ReservaSummaryDto>.Success(summary);
    }

    public async Task<Result<IEnumerable<ReservaCalendarSlotDto>>> ObterCalendarioAsync(
        int condoId,
        int? areaComumId,
        DateTime inicio,
        DateTime fim,
        CancellationToken ct = default)
    {
        var reservas = await _reservaRepository.GetAllAsync(condoId, areaComumId: areaComumId, dataInicio: inicio, dataFim: fim, ct: ct);
        var calendarSlots = reservas
            .Where(x => x.Status != StatusReserva.Cancelada && x.Status != StatusReserva.Rejeitada)
            .Select(x => new ReservaCalendarSlotDto(
                ReservaId: x.Id,
                AreaComumId: x.AreaComumId,
                NomeAreaComum: x.AreaComum?.Nome ?? $"Área #{x.AreaComumId}",
                NomeMorador: x.NomeMorador,
                UnidadeMorador: x.UnidadeMorador,
                DataInicio: x.DataInicio,
                DataFim: x.DataFim,
                Status: x.Status));

        return Result<IEnumerable<ReservaCalendarSlotDto>>.Success(calendarSlots);
    }

    private static ReservaDto MapToDto(Reserva reserva)
    {
        return new ReservaDto(
            reserva.Id,
            reserva.TenantId,
            reserva.CondoId,
            reserva.AreaComumId,
            reserva.AreaComum?.Nome ?? string.Empty,
            reserva.MoradorId,
            reserva.NomeMorador,
            reserva.UnidadeMorador,
            reserva.DataInicio,
            reserva.DataFim,
            reserva.QuantidadePessoas,
            reserva.ValorTaxaReserva,
            reserva.ValorTaxaLimpeza,
            reserva.ValorTotal,
            reserva.Status,
            reserva.Observacao,
            reserva.MotivoCancelamento,
            reserva.DataCriacao);
    }
}
