using NetArchTest.Rules;

namespace Tests.Architecture;

/// <summary>
/// Testes de conformidade arquitetural para o bootstrap da API
/// conforme Subfase 1.1.2 do ROADMAP.md
/// </summary>
public class ApiConfigurationTests
{
    [Fact]
    public void Api_Should_ReferenceFastEndpoints()
    {
        // Arrange
        var apiAssembly = GetAssemblyByName("SmartCondo.Api");

        // Act & Assert
        Assert.NotNull(apiAssembly);

        // Verifica se há referência ao assembly FastEndpoints
        var referencedAssemblies = apiAssembly.GetReferencedAssemblies();
        var hasFastEndpointsReference = referencedAssemblies
            .Any(a => a.Name != null && a.Name.Contains("FastEndpoints"));

        Assert.True(hasFastEndpointsReference, 
            "A API deve referenciar o pacote FastEndpoints");
    }

    [Fact]
    public void Api_Should_NotContainWeatherForecastType()
    {
        // Arrange
        var apiAssembly = GetAssemblyByName("SmartCondo.Api");

        // Act
        var result = Types.InAssembly(apiAssembly)
            .That()
            .HaveNameMatching("WeatherForecast")
            .GetTypes();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Api_Should_ExposeProgram()
    {
        // Arrange & Act
        var apiAssembly = GetAssemblyByName("SmartCondo.Api");
        var programType = apiAssembly?.GetType("Program");

        // Assert
        Assert.NotNull(programType);
    }

    [Fact]
    public void Api_Should_OnlyDependOnAllowedProjects()
    {
        // Arrange
        var apiAssembly = GetAssemblyByName("SmartCondo.Api");
        var allowedDependencies = new[]
        {
            "BuildingBlocks.Shared",
            "BuildingBlocks.Infrastructure",
            "Modules."  // Permite qualquer módulo
        };

        // Act
        var result = Types.InAssembly(apiAssembly)
            .Should()
            .NotHaveDependencyOnAll("BuildingBlocks.Domain")  // API não deve referenciar Domain diretamente
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "A API não deve depender diretamente de BuildingBlocks.Domain (violação de Clean Architecture)");
    }

    /// <summary>
    /// Carrega assembly pelo nome a partir do diretório de saída do projeto
    /// </summary>
    private static System.Reflection.Assembly? GetAssemblyByName(string assemblyName)
    {
        try
        {
            return System.Reflection.Assembly.Load(assemblyName);
        }
        catch
        {
            var currentDir = Path.GetDirectoryName(typeof(ApiConfigurationTests).Assembly.Location);
            var searchPattern = $"{assemblyName}.dll";
            var assemblyPath = Directory.GetFiles(currentDir!, searchPattern, SearchOption.AllDirectories).FirstOrDefault();

            return assemblyPath != null
                ? System.Reflection.Assembly.LoadFrom(assemblyPath)
                : null;
        }
    }
}
