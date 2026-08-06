using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;

namespace Modules.Operations.Domain.Entities;

public class PlanoManutencao : ITenantScoped
{
    public Guid Id { get; private set; }
    public int TenantId { get; set; }
    public int CondoId { get; private set; }
    public string Titulo { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public CategoriaManutencao Categoria { get; private set; }
    public PeriodicidadeManutencao Periodicidade { get; private set; }
    public DateTime? DataUltimaManutencao { get; private set; }
    public DateTime DataProximaManutencao { get; private set; }
    public StatusManutencao Status { get; private set; }
    public string? ResponsavelTecnico { get; private set; }
    public string? EmpresaContratada { get; private set; }
    public decimal? CustoEstimado { get; private set; }
    public decimal? CustoReal { get; private set; }
    public string? Observacoes { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    // EF Core Constructor
    private PlanoManutencao() { }

    public static PlanoManutencao Create(
        int tenantId,
        int condoId,
        string titulo,
        CategoriaManutencao categoria,
        PeriodicidadeManutencao periodicidade,
        DateTime dataProximaManutencao,
        string? descricao = null,
        string? responsavelTecnico = null,
        string? empresaContratada = null,
        decimal? custoEstimado = null,
        DateTime? dataUltimaManutencao = null,
        string? observacoes = null)
    {
        if (tenantId <= 0)
            throw new PlanoManutencaoDomainException("TenantId é obrigatório e deve ser maior que zero.");

        if (condoId <= 0)
            throw new PlanoManutencaoDomainException("CondoId é obrigatório e deve ser maior que zero.");

        if (string.IsNullOrWhiteSpace(titulo))
            throw new PlanoManutencaoDomainException("O título da manutenção não pode ser vazio.");

        if (titulo.Length > 150)
            throw new PlanoManutencaoDomainException("O título da manutenção deve ter no máximo 150 caracteres.");

        if (custoEstimado.HasValue && custoEstimado.Value < 0)
            throw new PlanoManutencaoDomainException("O custo estimado não pode ser negativo.");

        var plano = new PlanoManutencao
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CondoId = condoId,
            Titulo = titulo.Trim(),
            Descricao = descricao?.Trim(),
            Categoria = categoria,
            Periodicidade = periodicidade,
            DataUltimaManutencao = dataUltimaManutencao.HasValue ? EnsureUtc(dataUltimaManutencao.Value.Date) : null,
            DataProximaManutencao = EnsureUtc(dataProximaManutencao.Date),
            ResponsavelTecnico = responsavelTecnico?.Trim(),
            EmpresaContratada = empresaContratada?.Trim(),
            CustoEstimado = custoEstimado,
            Observacoes = observacoes?.Trim(),
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        plano.CalcularStatus(DateTime.Today);
        return plano;
    }

    public void CalcularStatus(DateTime dataReferencia)
    {
        if (Status == StatusManutencao.Cancelada)
            return;

        var refDate = dataReferencia.Date;
        var proximaDate = DataProximaManutencao.Date;

        if (proximaDate < refDate)
        {
            Status = StatusManutencao.Atrasada;
        }
        else if (proximaDate <= refDate.AddDays(15))
        {
            Status = StatusManutencao.Proxima;
        }
        else
        {
            Status = StatusManutencao.EmDia;
        }
    }

    public void ConcluirManutencao(
        DateTime dataRealizacao,
        decimal? custoReal = null,
        string? observacoes = null,
        bool agendarProxima = true)
    {
        if (custoReal.HasValue && custoReal.Value < 0)
            throw new PlanoManutencaoDomainException("O custo real não pode ser negativo.");

        DataUltimaManutencao = EnsureUtc(dataRealizacao.Date);
        if (custoReal.HasValue)
            CustoReal = custoReal.Value;

        if (!string.IsNullOrWhiteSpace(observacoes))
        {
            Observacoes = string.IsNullOrWhiteSpace(Observacoes)
                ? observacoes.Trim()
                : $"{Observacoes}\n[{dataRealizacao:dd/MM/yyyy}]: {observacoes.Trim()}";
        }

        if (agendarProxima)
        {
            DataProximaManutencao = EnsureUtc(CalcularProximaData(dataRealizacao.Date, Periodicidade));
            CalcularStatus(DateTime.Today);
        }
        else
        {
            Status = StatusManutencao.Concluida;
        }

        DataAtualizacao = DateTime.UtcNow;
    }

    private static DateTime EnsureUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    public void Atualizar(
        string titulo,
        string? descricao,
        CategoriaManutencao categoria,
        PeriodicidadeManutencao periodicidade,
        DateTime dataProximaManutencao,
        string? responsavelTecnico,
        string? empresaContratada,
        decimal? custoEstimado,
        string? observacoes)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new PlanoManutencaoDomainException("O título da manutenção não pode ser vazio.");

        if (titulo.Length > 150)
            throw new PlanoManutencaoDomainException("O título da manutenção deve ter no máximo 150 caracteres.");

        if (custoEstimado.HasValue && custoEstimado.Value < 0)
            throw new PlanoManutencaoDomainException("O custo estimado não pode ser negativo.");

        Titulo = titulo.Trim();
        Descricao = descricao?.Trim();
        Categoria = categoria;
        Periodicidade = periodicidade;
        DataProximaManutencao = EnsureUtc(dataProximaManutencao.Date);
        ResponsavelTecnico = responsavelTecnico?.Trim();
        EmpresaContratada = empresaContratada?.Trim();
        CustoEstimado = custoEstimado;
        Observacoes = observacoes?.Trim();
        DataAtualizacao = DateTime.UtcNow;

        CalcularStatus(DateTime.Today);
    }

    public void Desativar()
    {
        Ativo = false;
        Status = StatusManutencao.Cancelada;
        DataAtualizacao = DateTime.UtcNow;
    }

    public static DateTime CalcularProximaData(DateTime dataBase, PeriodicidadeManutencao periodicidade)
    {
        return periodicidade switch
        {
            PeriodicidadeManutencao.Semanal => dataBase.AddDays(7),
            PeriodicidadeManutencao.Mensal => dataBase.AddMonths(1),
            PeriodicidadeManutencao.Bimestral => dataBase.AddMonths(2),
            PeriodicidadeManutencao.Trimestral => dataBase.AddMonths(3),
            PeriodicidadeManutencao.Semestral => dataBase.AddMonths(6),
            PeriodicidadeManutencao.Anual => dataBase.AddYears(1),
            _ => dataBase.AddMonths(1)
        };
    }
}
