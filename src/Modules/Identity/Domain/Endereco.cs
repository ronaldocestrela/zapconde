namespace Modules.Identity.Domain;

public class Endereco
{
    public string Cep { get; set; } = string.Empty;

    public string Logradouro { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    public static string NormalizeCep(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());

    public static bool IsValidCep(string? value)
    {
        var cep = NormalizeCep(value);
        return cep.Length == 8;
    }
}
