using NetArchTest.Rules;

namespace Tests.Architecture;

/// &lt;summary&gt;
/// Testes de conformidade estrutural para validar a arquitetura do Modular Monolith
/// conforme especificado em AGENTS.md e ROADMAP.md - Subfase 1.1.1
/// &lt;/summary&gt;
public class StructuralConformityTests
{
    private const string BuildingBlocksDomain = "BuildingBlocks.Domain";
    private const string BuildingBlocksInfrastructure = "BuildingBlocks.Infrastructure";
    private const string BuildingBlocksShared = "BuildingBlocks.Shared";

    [Fact]
    public void BuildingBlocks_Domain_Should_Exist()
    {
        // Arrange &amp; Act
        var assembly = GetAssemblyByName(BuildingBlocksDomain);

        // Assert
        Assert.NotNull(assembly);
    }

    [Fact]
    public void BuildingBlocks_Infrastructure_Should_Exist()
    {
        // Arrange &amp; Act
        var assembly = GetAssemblyByName(BuildingBlocksInfrastructure);

        // Assert
        Assert.NotNull(assembly);
    }

    [Fact]
    public void BuildingBlocks_Shared_Should_Exist()
    {
        // Arrange &amp; Act
        var assembly = GetAssemblyByName(BuildingBlocksShared);

        // Assert
        Assert.NotNull(assembly);
    }

    [Theory]
    [InlineData("Modules.Identity")]
    [InlineData("Modules.Financial")]
    [InlineData("Modules.Operations")]
    [InlineData("Modules.AccessControl")]
    [InlineData("Modules.WhatsApp")]
    [InlineData("Modules.AIEngine")]
    public void Module_Should_Exist(string moduleName)
    {
        // Arrange &amp; Act
        var assembly = GetAssemblyByName(moduleName);

        // Assert
        Assert.NotNull(assembly);
    }

    [Fact]
    public void SmartCondo_Api_Should_Exist()
    {
        // Arrange &amp; Act
        var assembly = GetAssemblyByName("SmartCondo.Api");

        // Assert
        Assert.NotNull(assembly);
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        // Arrange
        var domainAssembly = GetAssemblyByName(BuildingBlocksDomain);

        // Act
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn(BuildingBlocksInfrastructure)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            "Domain layer must not depend on Infrastructure layer (Clean Architecture violation)");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Api()
    {
        // Arrange
        var domainAssembly = GetAssemblyByName(BuildingBlocksDomain);

        // Act
        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("SmartCondo.Api")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            "Domain layer must not depend on API layer (Clean Architecture violation)");
    }

    /// &lt;summary&gt;
    /// Carrega assembly pelo nome a partir do diretório de saída do projeto
    /// &lt;/summary&gt;
    private static System.Reflection.Assembly? GetAssemblyByName(string assemblyName)
    {
        try
        {
            // Tenta carregar o assembly pelo nome simples
            return System.Reflection.Assembly.Load(assemblyName);
        }
        catch
        {
            // Se falhar, procura no diretório de bin local
            var currentDir = Path.GetDirectoryName(typeof(StructuralConformityTests).Assembly.Location);
            var searchPattern = $"{assemblyName}.dll";
            var assemblyPath = Directory.GetFiles(currentDir!, searchPattern, SearchOption.AllDirectories).FirstOrDefault();

            return assemblyPath != null 
                ? System.Reflection.Assembly.LoadFrom(assemblyPath) 
                : null;
        }
    }
}
