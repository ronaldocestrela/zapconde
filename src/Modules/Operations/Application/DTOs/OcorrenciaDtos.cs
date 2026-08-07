using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.DTOs;

public record OcorrenciaDto(
    Guid Id,
    int TenantId,
    int CondoId,
    string MoradorId,
    string MoradorNome,
    string Titulo,
    string Descricao,
    CategoriaOcorrencia Categoria,
    PrioridadeOcorrencia Prioridade,
    StatusOcorrencia Status,
    string Localizacao,
    DateTime DataAbertura,
    DateTime? DataConclusao,
    string? ResponsavelId,
    string? ResponsavelNome,
    string? ObservacaoResolucao,
    IReadOnlyCollection<AnexoOcorrenciaDto> Anexos,
    IReadOnlyCollection<HistoricoOcorrenciaDto> Historico,
    string? OrigemTriagemIa = null,
    string? ResumoTriagemIa = null,
    double? ConfiancaTriagemIa = null,
    string? AudioUrl = null,
    string? TranscricaoAudio = null,
    string? SetorResponsavelSugerido = null
);

public record AnexoOcorrenciaDto(
    Guid Id,
    Guid OcorrenciaId,
    string Url,
    string NomeArquivo,
    string ContentType,
    long TamanhoBytes,
    DateTime DataUpload,
    string UploadPorUserId
);

public record HistoricoOcorrenciaDto(
    Guid Id,
    Guid OcorrenciaId,
    StatusOcorrencia? StatusAnterior,
    StatusOcorrencia StatusNovo,
    string Comentario,
    DateTime DataAlteracao,
    string AlteradoPorUserId,
    string AlteradoPorNome
);

public record OcorrenciaSummaryDto(
    int Total,
    int Abertas,
    int EmAndamento,
    int Resolvidas,
    int Urgentes
);

public record CriarOcorrenciaRequest(
    int CondoId,
    string MoradorId,
    string MoradorNome,
    string Titulo,
    string Descricao,
    CategoriaOcorrencia Categoria,
    PrioridadeOcorrencia Prioridade,
    string Localizacao,
    List<CriarAnexoDto>? AnexosIniciais = null,
    string? OrigemTriagemIa = null,
    string? ResumoTriagemIa = null,
    double? ConfiancaTriagemIa = null,
    string? AudioUrl = null,
    string? TranscricaoAudio = null,
    string? SetorResponsavelSugerido = null
);

public record CriarAnexoDto(
    string Url,
    string NomeArquivo,
    string ContentType,
    long TamanhoBytes
);

public record AtualizarStatusOcorrenciaRequest(
    StatusOcorrencia NovoStatus,
    string Comentario,
    string UsuarioId,
    string UsuarioNome,
    string? ObservacaoResolucao = null
);

public record AdicionarAnexoOcorrenciaRequest(
    string Url,
    string NomeArquivo,
    string ContentType,
    long TamanhoBytes,
    string UploadPorUserId
);

public record TriagemOcorrenciaRequestDto(
    string? FotoUrl,
    string? AudioUrl,
    string? RelatoTexto,
    string MoradorId = "morador-default",
    string MoradorNome = "Morador Residente",
    int CondoId = 1
);

public record ResultadoTriagemOcorrenciaDto(
    string TituloSugerido,
    string DescricaoDetalhada,
    CategoriaOcorrencia CategoriaInferida,
    PrioridadeOcorrencia PrioridadeInferida,
    string LocalizacaoSugerida,
    string SetorResponsavelSugerido,
    double NivelConfianca,
    string JustificativaIa,
    string OrigemTriagem,
    Guid? OcorrenciaCriadaId = null
);
