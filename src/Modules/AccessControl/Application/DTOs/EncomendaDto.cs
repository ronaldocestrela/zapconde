using Modules.AccessControl.Domain.Enums;

namespace Modules.AccessControl.Application.DTOs;

public record EncomendaDto(
    int Id,
    int TenantId,
    int CondoId,
    int UnidadeId,
    string BlocoUnidade,
    string CodigoRastreio,
    string Descricao,
    string? Remetente,
    string? Transportadora,
    string? LocalArmazenamento,
    TipoEncomenda Tipo,
    string TipoDescricao,
    StatusEncomenda Status,
    string StatusDescricao,
    DateTimeOffset DataRecebimento,
    string RecebidoPorNome,
    DateTimeOffset? DataRetirada,
    string? RetiradoPorNome,
    DateTimeOffset? NotificadoEm,
    string? Observacoes,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm);

public record RegistrarRecebimentoEncomendaRequest(
    int CondoId,
    int UnidadeId,
    string BlocoUnidade,
    string CodigoRastreio,
    string Descricao,
    string? Remetente,
    string? Transportadora,
    string? LocalArmazenamento,
    TipoEncomenda Tipo,
    string RecebidoPorNome,
    DateTimeOffset? DataRecebimento,
    string? Observacoes);

public record RegistrarBaixaEncomendaRequest(
    string RetiradoPorNome,
    DateTimeOffset? DataRetirada);

public record EncomendaSummaryDto(
    int TotalEncomendas,
    int AguardandoRetirada,
    int EntreguesHoje,
    int Pereciveis);
