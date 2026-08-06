using Modules.Operations.Domain.Enums;

namespace Modules.Operations.Application.DTOs;

public record PlanoManutencaoDto(
    Guid Id,
    int TenantId,
    int CondoId,
    string Titulo,
    string? Descricao,
    CategoriaManutencao Categoria,
    PeriodicidadeManutencao Periodicidade,
    DateTime? DataUltimaManutencao,
    DateTime DataProximaManutencao,
    StatusManutencao Status,
    string? ResponsavelTecnico,
    string? EmpresaContratada,
    decimal? CustoEstimado,
    decimal? CustoReal,
    string? Observacoes,
    bool Ativo,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
);

public record PlanoManutencaoSummaryDto(
    int Total,
    int EmDia,
    int Proximas,
    int Atrasadas,
    int EmExecucao,
    decimal TotalCustoEstimado,
    decimal TotalCustoReal
);

public record ManutencaoCalendarEventDto(
    Guid Id,
    string Titulo,
    CategoriaManutencao Categoria,
    StatusManutencao Status,
    DateTime Data,
    string? EmpresaContratada
);

public record CreatePlanoManutencaoRequest(
    int CondoId,
    string Titulo,
    CategoriaManutencao Categoria,
    PeriodicidadeManutencao Periodicidade,
    DateTime DataProximaManutencao,
    string? Descricao = null,
    string? ResponsavelTecnico = null,
    string? EmpresaContratada = null,
    decimal? CustoEstimado = null,
    DateTime? DataUltimaManutencao = null,
    string? Observacoes = null
);

public record UpdatePlanoManutencaoRequest(
    string Titulo,
    string? Descricao,
    CategoriaManutencao Categoria,
    PeriodicidadeManutencao Periodicidade,
    DateTime DataProximaManutencao,
    string? ResponsavelTecnico = null,
    string? EmpresaContratada = null,
    decimal? CustoEstimado = null,
    string? Observacoes = null
);

public record ConcluirManutencaoRequest(
    DateTime DataRealizacao,
    decimal? CustoReal = null,
    string? Observacoes = null,
    bool AgendarProxima = true
);
