namespace Modules.Identity.Domain;

/// <summary>
/// Value object para CNPJ brasileiro com normalização e validação de dígitos verificadores.
/// </summary>
public static class CnpjValidator
{
    private static readonly int[] Multiplier1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] Multiplier2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

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
        if (digits.Length != 14)
        {
            return false;
        }

        if (digits.All(c => c == digits[0]))
        {
            return false;
        }

        var numbers = digits.Select(c => c - '0').ToArray();
        var firstCheck = CalculateCheckDigit(numbers, Multiplier1);
        if (numbers[12] != firstCheck)
        {
            return false;
        }

        var secondCheck = CalculateCheckDigit(numbers, Multiplier2);
        return numbers[13] == secondCheck;
    }

    public static string Format(string? value)
    {
        var digits = Normalize(value);
        if (digits.Length != 14)
        {
            return digits;
        }

        return $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..]}";
    }

    private static int CalculateCheckDigit(int[] numbers, int[] multiplier)
    {
        var sum = 0;
        for (var i = 0; i < multiplier.Length; i++)
        {
            sum += numbers[i] * multiplier[i];
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
