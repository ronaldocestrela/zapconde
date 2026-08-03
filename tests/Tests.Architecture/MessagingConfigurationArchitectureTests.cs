using System.Text.Json;
using NetArchTest.Rules;

namespace Tests.Architecture;

/// <summary>
/// Testes de conformidade arquitetural da Subfase 1.3.1:
/// bootstrap de MassTransit com RabbitMQ e configuração obrigatória.
/// </summary>
public class MessagingConfigurationArchitectureTests
{
    [Fact]
    public void Infrastructure_Should_Reference_MassTransit()
    {
        var hasMassTransitReference = HasPackageReference("MassTransit");

        Assert.True(hasMassTransitReference,
            "BuildingBlocks.Infrastructure deve referenciar MassTransit na Subfase 1.3.1");
    }

    [Fact]
    public void Infrastructure_Should_Reference_MassTransitRabbitMq()
    {
        var hasMassTransitRabbitMqReference = HasPackageReference("MassTransit.RabbitMQ");

        Assert.True(hasMassTransitRabbitMqReference,
            "BuildingBlocks.Infrastructure deve referenciar MassTransit.RabbitMQ na Subfase 1.3.1");
    }

    [Fact]
    public void Infrastructure_Should_Contain_RabbitMqOptions_Type()
    {
        var infrastructureAssembly = GetAssemblyByName("BuildingBlocks.Infrastructure");

        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Infrastructure.Messaging")
            .And()
            .HaveNameMatching("RabbitMqOptions")
            .GetTypes();

        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Theory]
    [InlineData("src/API/SmartCondo.Api/appsettings.json")]
    [InlineData("src/API/SmartCondo.Api/appsettings.Development.json")]
    public void Api_AppSettings_Should_Contain_RabbitMq_Section_With_Required_Keys(string relativePath)
    {
        var filePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            relativePath));

        Assert.True(File.Exists(filePath), $"Arquivo não encontrado: {filePath}");

        var json = File.ReadAllText(filePath);
        using var document = JsonDocument.Parse(json);

        Assert.True(document.RootElement.TryGetProperty("RabbitMQ", out var rabbitMqSection),
            $"Arquivo {relativePath} deve conter seção RabbitMQ");

        Assert.True(rabbitMqSection.TryGetProperty("Host", out _), "RabbitMQ.Host é obrigatório");
        Assert.True(rabbitMqSection.TryGetProperty("Port", out _), "RabbitMQ.Port é obrigatório");
        Assert.True(rabbitMqSection.TryGetProperty("VirtualHost", out _), "RabbitMQ.VirtualHost é obrigatório");
        Assert.True(rabbitMqSection.TryGetProperty("Username", out _), "RabbitMQ.Username é obrigatório");
        Assert.True(rabbitMqSection.TryGetProperty("Password", out _), "RabbitMQ.Password é obrigatório");
    }

    private static bool HasPackageReference(string packageId)
    {
        var infrastructureProjectPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "BuildingBlocks", "BuildingBlocks.Infrastructure", "BuildingBlocks.Infrastructure.csproj"));

        Assert.True(File.Exists(infrastructureProjectPath),
            $"Arquivo de projeto não encontrado: {infrastructureProjectPath}");

        var projectXml = System.Xml.Linq.XDocument.Load(infrastructureProjectPath);
        var packageReferences = projectXml
            .Descendants()
            .Where(x => x.Name.LocalName == "PackageReference")
            .Select(x => (string?)x.Attribute("Include"))
            .Where(x => !string.IsNullOrWhiteSpace(x));

        return packageReferences.Any(x => string.Equals(x, packageId, StringComparison.Ordinal));
    }

    private static System.Reflection.Assembly GetAssemblyByName(string assemblyName)
    {
        try
        {
            return System.Reflection.Assembly.Load(assemblyName);
        }
        catch
        {
            var currentDir = Path.GetDirectoryName(typeof(MessagingConfigurationArchitectureTests).Assembly.Location);
            var searchPattern = $"{assemblyName}.dll";
            var assemblyPath = Directory.GetFiles(currentDir!, searchPattern, SearchOption.AllDirectories).FirstOrDefault();

            return assemblyPath != null
                ? System.Reflection.Assembly.LoadFrom(assemblyPath)
                : throw new FileNotFoundException($"Assembly {assemblyName} não encontrado.");
        }
    }
}
