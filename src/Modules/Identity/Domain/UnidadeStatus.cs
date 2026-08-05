using System.Text.Json.Serialization;

namespace Modules.Identity.Domain;

/// <summary>
/// Status operacional de uma unidade habitacional.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnidadeStatus
{
    Ocupada = 0,
    Vaga = 1,
    EmReforma = 2
}
