namespace Modules.Identity.Domain;

/// <summary>
/// Validador de CPF brasileiro com normalização e dígitos verificadores.
/// </summary>
public static class CpfValidator
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value.Where(char.IsDigit).ToArray());
    }

    public static bool IsValid(string? value)
    {
        var digits = Normalize(value);
        if (digits.Length != 11)
        {
            return false;
        }

        if (digits.All(c => c == digits[0]))
        {
            return false;
        }

        var numbers = digits.Select(c => c - '0').ToArray();

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += numbers[i] * (10 - i);
        }

        var remainder = sum % 11;
        var firstCheck = remainder < 2 ? 0 : 11 - remainder;
        if (numbers[9] != firstCheck)
        {
            return false;
        }

        sum = 0;
        for (var i = 0; i < 10; i++)
        {
            sum += numbers[i] * (11 - i);
        }

        remainder = sum % 11;
        var secondCheck = remainder < 2 ? 0 : 11 - remainder;
        return numbers[10] == secondCheck;
    }

    public static string Format(string? value)
    {
        var digits = Normalize(value);
        if (digits.Length != 11)
        {
            return digits;
        }

        return $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..]}";
    }
}
