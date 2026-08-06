using BuildingBlocks.Shared.MultiTenancy;
using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Domain.Entities;

/// <summary>
/// Aggregate Root de Pasta Digital de Prestação de Contas Mensal do Condomínio.
/// </summary>
public class PastaDigital : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int CondoId { get; set; }
    public int Ano { get; set; }
    public int Mes { get; set; }

    public StatusPastaDigital Status { get; set; } = StatusPastaDigital.Rascunho;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataFechamento { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public int? AprovadoPorUserId { get; set; }
    public string ObservacoesConselho { get; set; } = string.Empty;
    public string ResumoExecutivoIa { get; set; } = string.Empty;

    public decimal SaldoAnterior { get; set; }
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal SaldoMes { get; set; }
    public decimal SaldoAcumulado { get; set; }

    private readonly List<DocumentoPrestacaoContas> _documentos = new();
    public IReadOnlyCollection<DocumentoPrestacaoContas> Documentos => _documentos.AsReadOnly();

    private readonly List<ItemBalancete> _itensBalancete = new();
    public IReadOnlyCollection<ItemBalancete> ItensBalancete => _itensBalancete.AsReadOnly();

    protected PastaDigital() { }

    public static PastaDigital Create(
        int tenantId,
        int condoId,
        int ano,
        int mes,
        decimal saldoAnterior = 0,
        string resumoIa = "")
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId inválido.", nameof(tenantId));
        if (condoId <= 0) throw new ArgumentException("CondoId inválido.", nameof(condoId));
        if (ano < 2000 || ano > 2100) throw new ArgumentException("Ano inválido.", nameof(ano));
        if (mes < 1 || mes > 12) throw new ArgumentException("Mês inválido.", nameof(mes));

        return new PastaDigital
        {
            TenantId = tenantId,
            CondoId = condoId,
            Ano = ano,
            Mes = mes,
            SaldoAnterior = saldoAnterior,
            SaldoAcumulado = saldoAnterior,
            ResumoExecutivoIa = resumoIa,
            Status = StatusPastaDigital.Rascunho,
            DataCriacao = DateTime.UtcNow
        };
    }

    public ItemBalancete AdicionarItemBalancete(
        TipoLancamentoBalancete tipo,
        CategoriaPlanoContas categoria,
        string descricao,
        decimal valorOrcado,
        decimal valorRealizado,
        DateTime dataLancamento,
        bool conciliado = false)
    {
        if (Status == StatusPastaDigital.Publicada)
            throw new InvalidOperationException("Não é possível alterar uma pasta digital já publicada.");

        var item = ItemBalancete.Create(
            TenantId,
            Id,
            tipo,
            categoria,
            descricao,
            valorOrcado,
            valorRealizado,
            dataLancamento,
            conciliado);

        _itensBalancete.Add(item);
        RecalcularSaldos();
        return item;
    }

    public DocumentoPrestacaoContas AnexarDocumento(
        CategoriaDocumentoPrestacao categoria,
        string titulo,
        string nomeArquivo,
        string urlArquivo,
        string contentType,
        long tamanhoBytes,
        int uploadPorUserId)
    {
        if (Status == StatusPastaDigital.Publicada)
            throw new InvalidOperationException("Não é possível alterar uma pasta digital já publicada.");

        var doc = DocumentoPrestacaoContas.Create(
            TenantId,
            Id,
            categoria,
            titulo,
            nomeArquivo,
            urlArquivo,
            contentType,
            tamanhoBytes,
            uploadPorUserId);

        _documentos.Add(doc);
        return doc;
    }

    public void RecalcularSaldos()
    {
        TotalReceitas = _itensBalancete
            .Where(i => i.TipoLancamento == TipoLancamentoBalancete.Receita)
            .Sum(i => i.ValorRealizado);

        TotalDespesas = _itensBalancete
            .Where(i => i.TipoLancamento == TipoLancamentoBalancete.Despesa)
            .Sum(i => i.ValorRealizado);

        SaldoMes = TotalReceitas - TotalDespesas;
        SaldoAcumulado = SaldoAnterior + SaldoMes;
    }

    public void SubmeterParaConselho()
    {
        if (Status != StatusPastaDigital.Rascunho && Status != StatusPastaDigital.Rejeitada)
            throw new InvalidOperationException($"Não é possível submeter pasta digital no status {Status}.");

        RecalcularSaldos();
        Status = StatusPastaDigital.EmAnaliseConselho;
        DataFechamento = DateTime.UtcNow;
    }

    public void Aprovar(int aprovadoPorUserId, string parecer = "")
    {
        if (Status != StatusPastaDigital.EmAnaliseConselho)
            throw new InvalidOperationException("Apenas pastas em análise pelo conselho podem ser aprovadas.");

        Status = StatusPastaDigital.Aprovada;
        AprovadoPorUserId = aprovadoPorUserId;
        ObservacoesConselho = parecer;
        DataAprovacao = DateTime.UtcNow;
    }

    public void Rejeitar(string parecerMotivo)
    {
        if (Status != StatusPastaDigital.EmAnaliseConselho)
            throw new InvalidOperationException("Apenas pastas em análise pelo conselho podem ser rejeitadas.");

        Status = StatusPastaDigital.Rejeitada;
        ObservacoesConselho = parecerMotivo;
    }

    public void Publicar()
    {
        if (Status != StatusPastaDigital.Aprovada)
            throw new InvalidOperationException("Apenas pastas aprovadas pelo conselho podem ser publicadas.");

        Status = StatusPastaDigital.Publicada;
    }
}
