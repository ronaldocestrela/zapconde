using BuildingBlocks.Shared.MultiTenancy;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Domain.Exceptions;

namespace Modules.Operations.Domain.Entities;

public class Ocorrencia : ITenantScoped
{
    private readonly List<AnexoOcorrencia> _anexos = new();
    private readonly List<HistoricoOcorrencia> _historico = new();

    public Guid Id { get; private set; }
    public int TenantId { get; set; }
    public int CondoId { get; private set; }
    public string MoradorId { get; private set; } = string.Empty;
    public string MoradorNome { get; private set; } = string.Empty;
    public string Titulo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public CategoriaOcorrencia Categoria { get; private set; }
    public PrioridadeOcorrencia Prioridade { get; private set; }
    public StatusOcorrencia Status { get; private set; }
    public string Localizacao { get; private set; } = string.Empty;
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataConclusao { get; private set; }
    public string? ResponsavelId { get; private set; }
    public string? ResponsavelNome { get; private set; }
    public string? ObservacaoResolucao { get; private set; }

    // Rastreabilidade de Triagem Inteligente via IA / Semantic Kernel
    public string? OrigemTriagemIa { get; private set; }
    public string? ResumoTriagemIa { get; private set; }
    public double? ConfiancaTriagemIa { get; private set; }
    public string? AudioUrl { get; private set; }
    public string? TranscricaoAudio { get; private set; }
    public string? SetorResponsavelSugerido { get; private set; }

    public IReadOnlyCollection<AnexoOcorrencia> Anexos => _anexos.AsReadOnly();
    public IReadOnlyCollection<HistoricoOcorrencia> Historico => _historico.AsReadOnly();

    // EF Core Constructor
    private Ocorrencia() { }

    public static Ocorrencia Create(
        int tenantId,
        int condoId,
        string moradorId,
        string moradorNome,
        string titulo,
        string descricao,
        CategoriaOcorrencia categoria,
        PrioridadeOcorrencia prioridade,
        string localizacao)
    {
        if (tenantId <= 0) throw new ArgumentException("TenantId deve ser maior que zero.", nameof(tenantId));
        if (condoId <= 0) throw new ArgumentException("CondoId deve ser maior que zero.", nameof(condoId));
        if (string.IsNullOrWhiteSpace(moradorId)) throw new ArgumentException("MoradorId é obrigatório.", nameof(moradorId));
        if (string.IsNullOrWhiteSpace(titulo)) throw new ArgumentException("Título da ocorrência é obrigatório.", nameof(titulo));
        if (string.IsNullOrWhiteSpace(descricao)) throw new ArgumentException("Descrição da ocorrência é obrigatória.", nameof(descricao));

        var ocorrencia = new Ocorrencia
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CondoId = condoId,
            MoradorId = moradorId,
            MoradorNome = string.IsNullOrWhiteSpace(moradorNome) ? "Morador Anônimo" : moradorNome,
            Titulo = titulo.Trim(),
            Descricao = descricao.Trim(),
            Categoria = categoria,
            Prioridade = prioridade,
            Status = StatusOcorrencia.Aberta,
            Localizacao = string.IsNullOrWhiteSpace(localizacao) ? "Não especificada" : localizacao.Trim(),
            DataAbertura = DateTime.UtcNow
        };

        var historicoInicial = HistoricoOcorrencia.Create(
            tenantId: tenantId,
            condoId: condoId,
            ocorrenciaId: ocorrencia.Id,
            statusAnterior: null,
            statusNovo: StatusOcorrencia.Aberta,
            comentario: "Ocorrencia aberta pelo morador",
            alteradoPorUserId: moradorId,
            alteradoPorNome: ocorrencia.MoradorNome
        );

        ocorrencia._historico.Add(historicoInicial);

        return ocorrencia;
    }

    public AnexoOcorrencia AdicionarAnexo(string url, string nomeArquivo, string contentType, long tamanhoBytes, string uploadPorUserId)
    {
        var anexo = AnexoOcorrencia.Create(
            tenantId: TenantId,
            condoId: CondoId,
            ocorrenciaId: Id,
            url: url,
            nomeArquivo: nomeArquivo,
            contentType: contentType,
            tamanhoBytes: tamanhoBytes,
            uploadPorUserId: uploadPorUserId
        );

        _anexos.Add(anexo);
        return anexo;
    }

    public void RemoverAnexo(Guid anexoId)
    {
        var anexo = _anexos.FirstOrDefault(a => a.Id == anexoId);
        if (anexo != null)
        {
            _anexos.Remove(anexo);
        }
    }

    public void AtribuirResponsavel(string responsavelId, string responsavelNome)
    {
        if (string.IsNullOrWhiteSpace(responsavelId)) throw new ArgumentException("ResponsavelId é obrigatório.", nameof(responsavelId));

        ResponsavelId = responsavelId;
        ResponsavelNome = string.IsNullOrWhiteSpace(responsavelNome) ? "Atendente" : responsavelNome.Trim();
    }

    public void AssociarTriagemIa(
        string origem,
        string resumo,
        double confianca,
        string? setorSugerido = null,
        string? audioUrl = null,
        string? transcricaoAudio = null)
    {
        OrigemTriagemIa = string.IsNullOrWhiteSpace(origem) ? "IA_Multimodal" : origem.Trim();
        ResumoTriagemIa = resumo?.Trim();
        ConfiancaTriagemIa = Math.Clamp(confianca, 0.0, 1.0);
        SetorResponsavelSugerido = setorSugerido?.Trim();
        if (!string.IsNullOrWhiteSpace(audioUrl)) AudioUrl = audioUrl.Trim();
        if (!string.IsNullOrWhiteSpace(transcricaoAudio)) TranscricaoAudio = transcricaoAudio.Trim();
    }

    public void AtualizarStatus(
        StatusOcorrencia novoStatus,
        string comentario,
        string usuarioId,
        string usuarioNome,
        string? observacaoResolucao = null)
    {
        if (Status == novoStatus) return;

        ValidarTransicaoStatus(Status, novoStatus);

        var statusAnterior = Status;
        Status = novoStatus;

        if (novoStatus == StatusOcorrencia.Resolvida || novoStatus == StatusOcorrencia.Cancelada)
        {
            DataConclusao = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(observacaoResolucao))
            {
                ObservacaoResolucao = observacaoResolucao.Trim();
            }
        }

        var itemHistorico = HistoricoOcorrencia.Create(
            tenantId: TenantId,
            condoId: CondoId,
            ocorrenciaId: Id,
            statusAnterior: statusAnterior,
            statusNovo: novoStatus,
            comentario: comentario ?? string.Empty,
            alteradoPorUserId: usuarioId,
            alteradoPorNome: usuarioNome
        );

        _historico.Add(itemHistorico);
    }

    private static void ValidarTransicaoStatus(StatusOcorrencia atual, StatusOcorrencia novo)
    {
        var eValido = (atual, novo) switch
        {
            (StatusOcorrencia.Aberta, StatusOcorrencia.EmAndamento) => true,
            (StatusOcorrencia.Aberta, StatusOcorrencia.Cancelada) => true,

            (StatusOcorrencia.EmAndamento, StatusOcorrencia.AguardandoPeca) => true,
            (StatusOcorrencia.EmAndamento, StatusOcorrencia.Resolvida) => true,
            (StatusOcorrencia.EmAndamento, StatusOcorrencia.Cancelada) => true,

            (StatusOcorrencia.AguardandoPeca, StatusOcorrencia.EmAndamento) => true,
            (StatusOcorrencia.AguardandoPeca, StatusOcorrencia.Resolvida) => true,
            (StatusOcorrencia.AguardandoPeca, StatusOcorrencia.Cancelada) => true,

            _ => false
        };

        if (!eValido)
        {
            throw new InvalidOcorrenciaStatusTransitionException(atual, novo);
        }
    }
}
