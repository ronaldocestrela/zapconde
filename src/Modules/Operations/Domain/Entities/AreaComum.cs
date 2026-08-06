using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Domain.Entities;

/// <summary>
/// Entidade raiz de domínio representando uma Área Comum (Salão de Festas, Churrasqueira, etc.) com isolamento multi-tenant.
/// </summary>
public class AreaComum : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoAreaComum Tipo { get; set; } = TipoAreaComum.Lazer;
    public StatusAreaComum Status { get; set; } = StatusAreaComum.Ativa;

    public int CapacidadeMaxima { get; set; }
    public decimal TaxaReserva { get; set; }
    public decimal TaxaLimpeza { get; set; }

    public TimeSpan HorarioInicioFuncionamento { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan HorarioFimFuncionamento { get; set; } = new TimeSpan(22, 0, 0);

    public int TempoAntecedenciaMinimaDias { get; set; } = 1;
    public int TempoAntecedenciaMaximaDias { get; set; } = 60;

    public bool RequerAprovacaoSindico { get; set; } = false;
    public string RegrasUso { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAtualizacao { get; set; }

    public decimal CustoTotalReserva => TaxaReserva + TaxaLimpeza;

    protected AreaComum() { }

    public static AreaComum Create(
        int tenantId,
        int condoId,
        string nome,
        string descricao,
        TipoAreaComum tipo,
        int capacidadeMaxima,
        decimal taxaReserva,
        decimal taxaLimpeza,
        TimeSpan horarioInicioFuncionamento,
        TimeSpan horarioFimFuncionamento,
        int tempoAntecedenciaMinimaDias = 1,
        int tempoAntecedenciaMaximaDias = 60,
        bool requerAprovacaoSindico = false,
        string regrasUso = "")
    {
        if (tenantId <= 0)
            throw new ArgumentException("TenantId é obrigatório.", nameof(tenantId));

        if (condoId <= 0)
            throw new ArgumentException("CondoId é obrigatório.", nameof(condoId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da área comum é obrigatório.", nameof(nome));

        if (capacidadeMaxima <= 0)
            throw new ArgumentException("A capacidade máxima deve ser maior que zero.", nameof(capacidadeMaxima));

        if (taxaReserva < 0)
            throw new ArgumentException("A taxa de reserva não pode ser negativa.", nameof(taxaReserva));

        if (taxaLimpeza < 0)
            throw new ArgumentException("A taxa de limpeza não pode ser negativa.", nameof(taxaLimpeza));

        if (horarioInicioFuncionamento >= horarioFimFuncionamento)
            throw new ArgumentException("O horário de início do funcionamento deve ser anterior ao horário de término.", nameof(horarioInicioFuncionamento));

        if (tempoAntecedenciaMinimaDias < 0)
            throw new ArgumentException("A antecedência mínima em dias não pode ser negativa.", nameof(tempoAntecedenciaMinimaDias));

        if (tempoAntecedenciaMaximaDias < tempoAntecedenciaMinimaDias)
            throw new ArgumentException("A antecedência máxima deve ser maior ou igual à antecedência mínima.", nameof(tempoAntecedenciaMaximaDias));

        return new AreaComum
        {
            TenantId = tenantId,
            CondoId = condoId,
            Nome = nome.Trim(),
            Descricao = descricao?.Trim() ?? string.Empty,
            Tipo = tipo,
            Status = StatusAreaComum.Ativa,
            CapacidadeMaxima = capacidadeMaxima,
            TaxaReserva = taxaReserva,
            TaxaLimpeza = taxaLimpeza,
            HorarioInicioFuncionamento = horarioInicioFuncionamento,
            HorarioFimFuncionamento = horarioFimFuncionamento,
            TempoAntecedenciaMinimaDias = tempoAntecedenciaMinimaDias,
            TempoAntecedenciaMaximaDias = tempoAntecedenciaMaximaDias,
            RequerAprovacaoSindico = requerAprovacaoSindico,
            RegrasUso = regrasUso?.Trim() ?? string.Empty,
            DataCriacao = DateTime.UtcNow
        };
    }

    public void AtualizarDados(
        string nome,
        string descricao,
        TipoAreaComum tipo,
        int capacidadeMaxima,
        TimeSpan horarioInicio,
        TimeSpan horarioFim,
        bool requerAprovacaoSindico,
        string regrasUso)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome da área comum é obrigatório.", nameof(nome));

        if (capacidadeMaxima <= 0)
            throw new ArgumentException("A capacidade máxima deve ser maior que zero.", nameof(capacidadeMaxima));

        if (horarioInicio >= horarioFim)
            throw new ArgumentException("O horário de início do funcionamento deve ser anterior ao horário de término.", nameof(horarioInicio));

        Nome = nome.Trim();
        Descricao = descricao?.Trim() ?? string.Empty;
        Tipo = tipo;
        CapacidadeMaxima = capacidadeMaxima;
        HorarioInicioFuncionamento = horarioInicio;
        HorarioFimFuncionamento = horarioFim;
        RequerAprovacaoSindico = requerAprovacaoSindico;
        RegrasUso = regrasUso?.Trim() ?? string.Empty;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarRegrasECustos(
        decimal taxaReserva,
        decimal taxaLimpeza,
        int tempoAntecedenciaMinimaDias,
        int tempoAntecedenciaMaximaDias)
    {
        if (taxaReserva < 0)
            throw new ArgumentException("A taxa de reserva não pode ser negativa.", nameof(taxaReserva));

        if (taxaLimpeza < 0)
            throw new ArgumentException("A taxa de limpeza não pode ser negativa.", nameof(taxaLimpeza));

        if (tempoAntecedenciaMinimaDias < 0)
            throw new ArgumentException("A antecedência mínima não pode ser negativa.", nameof(tempoAntecedenciaMinimaDias));

        if (tempoAntecedenciaMaximaDias < tempoAntecedenciaMinimaDias)
            throw new ArgumentException("A antecedência máxima deve ser maior ou igual à antecedência mínima.", nameof(tempoAntecedenciaMaximaDias));

        TaxaReserva = taxaReserva;
        TaxaLimpeza = taxaLimpeza;
        TempoAntecedenciaMinimaDias = tempoAntecedenciaMinimaDias;
        TempoAntecedenciaMaximaDias = tempoAntecedenciaMaximaDias;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AlterarStatus(StatusAreaComum novoStatus)
    {
        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }

    public bool ValidarElegibilidadeCapacidade(int quantidadePessoas)
    {
        return quantidadePessoas > 0 && quantidadePessoas <= CapacidadeMaxima;
    }
}
