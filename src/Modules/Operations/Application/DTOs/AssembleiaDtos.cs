using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.DTOs;

public record VotoDto(
    Guid Id,
    Guid PautaId,
    string MoradorUserId,
    string UnidadeId,
    string OpcaoEscolhida,
    double PesoVoto,
    DateTime DataVoto
);

public record PautaDto(
    Guid Id,
    Guid AssembleiaId,
    string Titulo,
    string? Descricao,
    int Ordem,
    TipoVotacao TipoVotacao,
    StatusPauta Status,
    List<string> OpcoesDisponiveis,
    int TotalVotos,
    Dictionary<string, int> ContagemVotos,
    IReadOnlyCollection<VotoDto> Votos
);

public record AssembleiaDto(
    Guid Id,
    int TenantId,
    int CondoId,
    string Titulo,
    string? Descricao,
    TipoAssembleia Tipo,
    StatusAssembleia Status,
    DateTime DataInicio,
    DateTime DataFim,
    DateTime? DataEncerramento,
    string? AtaTexto,
    DateTime? AtaGeradaEm,
    string CriadoPorUserId,
    int TotalPautas,
    int QuorumUnidadesParticipantes,
    IReadOnlyCollection<PautaDto> Pautas,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
);

public record AssembleiaSummaryDto(
    int Total,
    int Agendadas,
    int EmAndamento,
    int Encerradas,
    int Canceladas,
    int TotalVotosRegistrados
);

public record CreatePautaInput(
    string Titulo,
    TipoVotacao TipoVotacao,
    string? Descricao = null,
    List<string>? OpcoesDisponiveis = null
);

public record CreateAssembleiaRequest(
    int CondoId,
    string Titulo,
    TipoAssembleia Tipo,
    DateTime DataInicio,
    DateTime DataFim,
    string CriadoPorUserId,
    string? Descricao = null,
    List<CreatePautaInput>? PautasInicial = null
);

public record RegistrarVotoRequest(
    string MoradorUserId,
    string UnidadeId,
    string OpcaoEscolhida,
    double PesoVoto = 1.0
);

public record UpdateAssembleiaStatusRequest(
    StatusAssembleia NovoStatus
);
