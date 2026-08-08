namespace BuildingBlocks.Infrastructure.Environment;

/// <summary>
/// Utilitário para carregamento de variáveis de ambiente a partir de arquivos .env no startup da aplicação.
/// </summary>
public static class EnvLoader
{
    private static readonly Dictionary<string, string> SmtpAliasMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SMTP_HOST"] = "Smtp__Host",
        ["SMTP_PORT"] = "Smtp__Port",
        ["SMTP_USERNAME"] = "Smtp__Username",
        ["SMTP_PASSWORD"] = "Smtp__Password",
        ["SMTP_FROM_EMAIL"] = "Smtp__FromEmail",
        ["SMTP_FROMEMAIL"] = "Smtp__FromEmail",
        ["SMTP_FROM_NAME"] = "Smtp__FromName",
        ["SMTP_FROMNAME"] = "Smtp__FromName",
        ["SMTP_ENABLE_START_TLS"] = "Smtp__EnableStartTls",
        ["SMTP_TIMEOUT_MS"] = "Smtp__TimeoutMilliseconds",
        ["SMTP_TIMEOUT_MILLISECONDS"] = "Smtp__TimeoutMilliseconds"
    };

    /// <summary>
    /// Procura e carrega o arquivo .env no diretório atual ou nos diretórios superiores (raiz da solução).
    /// </summary>
    /// <param name="searchDirectory">Diretório inicial de busca opcional.</param>
    /// <returns>True se o arquivo .env foi encontrado e carregado; caso contrário, False.</returns>
    public static bool Load(string? searchDirectory = null)
    {
        var filePath = FindEnvFile(searchDirectory);
        if (filePath is null) return false;

        LoadFromFile(filePath);
        return true;
    }

    /// <summary>
    /// Carrega as variáveis de ambiente a partir de um arquivo específico.
    /// </summary>
    /// <param name="filePath">Caminho completo do arquivo .env</param>
    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0) continue;

            var key = trimmed[..separatorIndex].Trim();
            var value = trimmed[(separatorIndex + 1)..].Trim();

            // Remove aspas duplas ou simples se envolverem o valor
            if ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                if (value.Length >= 2)
                {
                    value = value[1..^1];
                }
            }

            System.Environment.SetEnvironmentVariable(key, value);

            // Mapeia aliases amigáveis de SMTP para a convenção Smtp__* do ASP.NET Core
            if (SmtpAliasMap.TryGetValue(key, out var netCoreKey))
            {
                System.Environment.SetEnvironmentVariable(netCoreKey, value);
            }
        }
    }

    /// <summary>
    /// Procura recursivamente pelo arquivo .env subindo na hierarquia de diretórios.
    /// </summary>
    public static string? FindEnvFile(string? startDirectory = null)
    {
        var currentDir = !string.IsNullOrWhiteSpace(startDirectory) 
            ? new DirectoryInfo(startDirectory) 
            : new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDir is not null)
        {
            var envPath = Path.Combine(currentDir.FullName, ".env");
            if (File.Exists(envPath)) return envPath;

            currentDir = currentDir.Parent;
        }

        return null;
    }
}
