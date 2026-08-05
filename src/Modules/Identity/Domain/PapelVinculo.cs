using System.Text.Json.Serialization;

namespace Modules.Identity.Domain;

/// <summary>
/// Papel do morador em relação à unidade (distinto de SmartCondoRoles RBAC).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PapelVinculo
{
    Proprietario = 0,
    Inquilino = 1
}
