using System.Text.Json.Serialization;

namespace Modules.AccessControl.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusEncomenda
{
    AguardandoRetirada = 1,
    Entregue = 2,
    Devolvida = 3,
    Cancelada = 4
}
