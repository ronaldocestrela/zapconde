using System.Text.Json.Serialization;

namespace Modules.AccessControl.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TipoEncomenda
{
    Pacote = 1,
    Envelope = 2,
    Caixa = 3,
    Perecivel = 4,
    Outros = 5
}
