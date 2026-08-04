namespace Modules.Identity.Domain;

/// <summary>
/// Normaliza celulares brasileiros para E.164.
/// </summary>
public static class PhoneNumberValidator
{
    public static bool TryNormalizeBrazilianMobile(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("55", StringComparison.Ordinal) && digits.Length == 13)
        {
            digits = digits[2..];
        }

        if (digits.Length != 11 ||
            digits[0] == '0' ||
            digits[1] is '0' ||
            digits[2] != '9')
        {
            return false;
        }

        normalized = $"+55{digits}";
        return true;
    }

    public static string NormalizeBrazilianMobile(string value)
    {
        if (!TryNormalizeBrazilianMobile(value, out var normalized))
        {
            throw new DomainValidationException("Número de celular brasileiro inválido.");
        }

        return normalized;
    }

    public static string FormatBrazilianMobile(string e164)
    {
        var normalized = NormalizeBrazilianMobile(e164);
        var digits = normalized[3..];
        return $"+55 ({digits[..2]}) {digits.Substring(2, 5)}-{digits[7..]}";
    }
}
