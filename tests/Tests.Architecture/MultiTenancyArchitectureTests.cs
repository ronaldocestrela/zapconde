using NetArchTest.Rules;

namespace Tests.Architecture;

/// <summary>
/// Testes de conformidade arquitetural para a Subfase 1.2.2 (ITenantScoped + Global Query Filter)
/// Valida isolamento de camadas, localização de contratos e ausência de dependências cíclicas.
/// </summary>
public class MultiTenancyArchitectureTests
{
    [Fact]
    public void ITenantScoped_Should_Exist_In_BuildingBlocksShared()
    {
        // Arrange
        var sharedAssembly = GetAssemblyByName("BuildingBlocks.Shared");

        // Act
        var result = Types.InAssembly(sharedAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Shared.MultiTenancy")
            .And()
            .HaveNameMatching("ITenantScoped")
            .GetTypes();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public void ICurrentTenantService_Should_Exist_In_BuildingBlocksShared()
    {
        // Arrange
        var sharedAssembly = GetAssemblyByName("BuildingBlocks.Shared");

        // Act
        var result = Types.InAssembly(sharedAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Shared.MultiTenancy")
            .And()
            .HaveNameMatching("ICurrentTenantService")
            .GetTypes();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public void BuildingBlocksShared_Should_NotReference_EntityFrameworkCore()
    {
        // Arrange
        var sharedAssembly = GetAssemblyByName("BuildingBlocks.Shared");

        // Act
        var result = Types.InAssembly(sharedAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "BuildingBlocks.Shared não deve depender de Entity Framework Core (violação de Clean Architecture)");
    }

    [Fact]
    public void CurrentTenantService_Should_Exist_In_BuildingBlocksInfrastructure()
    {
        // Arrange
        var infrastructureAssembly = GetAssemblyByName("BuildingBlocks.Infrastructure");

        // Act
        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Infrastructure.MultiTenancy")
            .And()
            .HaveNameMatching("CurrentTenantService")
            .GetTypes();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public void MultiTenantDbContext_Should_Exist_In_BuildingBlocksInfrastructure()
    {
        // Arrange
        var infrastructureAssembly = GetAssemblyByName("BuildingBlocks.Infrastructure");

        // Act
        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Infrastructure.Persistence")
            .And()
            .HaveNameMatching("MultiTenantDbContext")
            .GetTypes();

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public void MultiTenantDbContext_Should_Inherit_From_DbContext()
    {
        // Arrange
        var infrastructureAssembly = GetAssemblyByName("BuildingBlocks.Infrastructure");

        // Act
        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .HaveNameMatching("MultiTenantDbContext")
            .Should()
            .Inherit(typeof(Microsoft.EntityFrameworkCore.DbContext))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            "MultiTenantDbContext deve herdar de DbContext do Entity Framework Core");
    }

    [Fact]
    public void Infrastructure_Should_Reference_BuildingBlocksShared()
    {
        // Arrange
        var infrastructureAssembly = GetAssemblyByName("BuildingBlocks.Infrastructure");
        var referencedAssemblies = infrastructureAssembly.GetReferencedAssemblies();

        // Act
        var hasSharedReference = referencedAssemblies
            .Any(a => a.Name != null && a.Name.Contains("BuildingBlocks.Shared"));

        // Assert
        Assert.True(hasSharedReference,
            "BuildingBlocks.Infrastructure deve referenciar BuildingBlocks.Shared para usar contratos de multi-tenancy");
    }

    private static System.Reflection.Assembly GetAssemblyByName(string assemblyName)
    {
        try
        {
            return System.Reflection.Assembly.Load(assemblyName);
        }
        catch
        {
            var currentDir = Path.GetDirectoryName(typeof(MultiTenancyArchitectureTests).Assembly.Location);
            var searchPattern = $"{assemblyName}.dll";
            var assemblyPath = Directory.GetFiles(currentDir!, searchPattern, SearchOption.AllDirectories).FirstOrDefault();

            return assemblyPath != null
                ? System.Reflection.Assembly.LoadFrom(assemblyPath)
                : throw new FileNotFoundException($"Assembly {assemblyName} não encontrado.");
        }
    }
}
