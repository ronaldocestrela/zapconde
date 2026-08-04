namespace Modules.Identity.Application.Dtos;

/// <summary>
/// Contexto de tenant/condomínio resolvido na requisição atual.
/// </summary>
public sealed record TenantContextDto(
    int? TenantId,
    int? CondoId,
    bool IsResolved);
