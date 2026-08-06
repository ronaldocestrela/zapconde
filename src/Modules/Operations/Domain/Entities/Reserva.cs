using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;

namespace Modules.Operations.Domain.Entities;

/// <summary>
/// Entidade de domínio representando uma Reserva de Área Comum com isolamento multi-tenant.
/// </summary>
public class Reserva : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }

    public int AreaComumId { get; set; }
    public AreaComum? AreaComum { get; set; }

    public int MoradorId { get; set; }
    public string NomeMorador { get; set; } = string.Empty;
    public string UnidadeMorador { get; set; } = string.Empty;

    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int QuantidadePessoas { get; set; }

    public decimal ValorTaxaReserva { get; set; }
    public decimal ValorTaxaLimpeza { get; set; }
    public decimal ValorTotal { get; set; }

    public StatusReserva Status { get; set; } = StatusReserva.PendenteAprovacao;
    public string Observacao { get; set; } = string.Empty;
    public string MotivoCancelamento { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }

    protected Reserva() { }

    public static Reserva Create(
        int tenantId,
        int condoId,
        AreaComum areaComum,
        int moradorId,
        string nomeMorador,
        string unidadeMorador,
        DateTime dataInicio,
        DateTime dataFim,
        int quantidadePessoas,
        string observacao = "",
        DateTime? dataReferenciaCalculo = null)
    {
        if (tenantId <= 0)
            throw new ArgumentException("TenantId é obrigatório.", nameof(tenantId));

        if (condoId <= 0)
            throw new ArgumentException("CondoId é obrigatório.", nameof(condoId));

        if (areaComum == null)
            throw new ArgumentNullException(nameof(areaComum), "A área comum deve ser especificada.");

        if (moradorId <= 0)
            throw new ArgumentException("MoradorId é obrigatório.", nameof(moradorId));

        if (dataInicio >= dataFim)
            throw new ArgumentException("A data/hora de início deve ser anterior à data/hora de término.", nameof(dataInicio));

        if (areaComum.Status != StatusAreaComum.Ativa)
            throw new InvalidOperationException($"A área comum '{areaComum.Nome}' não está ativa para novas reservas.");

        if (!areaComum.ValidarElegibilidadeCapacidade(quantidadePessoas))
            throw new ArgumentException($"A quantidade de pessoas ({quantidadePessoas}) excede a capacidade máxima permitida ({areaComum.CapacidadeMaxima}) ou é inválida.", nameof(quantidadePessoas));

        var horaInicio = dataInicio.TimeOfDay;
        var horaFim = dataFim.TimeOfDay;

        if (horaInicio < areaComum.HorarioInicioFuncionamento || horaFim > areaComum.HorarioFimFuncionamento)
        {
            throw new ArgumentException($"O horário solicitado ({horaInicio:hh\\:mm} - {horaFim:hh\\:mm}) está fora do horário de funcionamento ({areaComum.HorarioInicioFuncionamento:hh\\:mm} - {areaComum.HorarioFimFuncionamento:hh\\:mm}).");
        }

        var hoje = (dataReferenciaCalculo ?? DateTime.UtcNow).Date;
        var antecedenciaDias = (dataInicio.Date - hoje).Days;

        if (antecedenciaDias < areaComum.TempoAntecedenciaMinimaDias)
        {
            throw new ArgumentException($"A reserva deve ser solicitada com no mínimo {areaComum.TempoAntecedenciaMinimaDias} dia(s) de antecedência.");
        }

        if (antecedenciaDias > areaComum.TempoAntecedenciaMaximaDias)
        {
            throw new ArgumentException($"A reserva não pode ser solicitada com mais de {areaComum.TempoAntecedenciaMaximaDias} dia(s) de antecedência.");
        }

        var statusInicial = areaComum.RequerAprovacaoSindico
            ? StatusReserva.PendenteAprovacao
            : StatusReserva.Confirmada;

        return new Reserva
        {
            TenantId = tenantId,
            CondoId = condoId,
            AreaComumId = areaComum.Id,
            AreaComum = areaComum,
            MoradorId = moradorId,
            NomeMorador = nomeMorador?.Trim() ?? $"Morador #{moradorId}",
            UnidadeMorador = unidadeMorador?.Trim() ?? "N/A",
            DataInicio = dataInicio,
            DataFim = dataFim,
            QuantidadePessoas = quantidadePessoas,
            ValorTaxaReserva = areaComum.TaxaReserva,
            ValorTaxaLimpeza = areaComum.TaxaLimpeza,
            ValorTotal = areaComum.CustoTotalReserva,
            Status = statusInicial,
            Observacao = observacao?.Trim() ?? string.Empty,
            DataCriacao = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Verifica se a reserva atual sobrepõe ao intervalo de tempo especificado.
    /// </summary>
    public bool Overlaps(DateTime inicio, DateTime fim)
    {
        if (Status == StatusReserva.Cancelada || Status == StatusReserva.Rejeitada)
            return false;

        return DataInicio < fim && DataFim > inicio;
    }

    public void Aprovar()
    {
        if (Status != StatusReserva.PendenteAprovacao)
            throw new InvalidOperationException("Apenas reservas pendentes de aprovação podem ser aprovadas.");

        Status = StatusReserva.Confirmada;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Rejeitar(string motivo)
    {
        if (Status != StatusReserva.PendenteAprovacao)
            throw new InvalidOperationException("Apenas reservas pendentes podem ser rejeitadas.");

        Status = StatusReserva.Rejeitada;
        MotivoCancelamento = motivo?.Trim() ?? "Solicitação rejeitada pela administração.";
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Cancelar(string motivo)
    {
        if (Status == StatusReserva.Cancelada || Status == StatusReserva.Rejeitada)
            throw new InvalidOperationException("A reserva já está cancelada ou rejeitada.");

        Status = StatusReserva.Cancelada;
        MotivoCancelamento = motivo?.Trim() ?? "Reserva cancelada.";
        DataAtualizacao = DateTime.UtcNow;
    }
}
