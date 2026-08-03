namespace Tests.Architecture;

/// <summary>
/// Testes de conformidade arquitetural para a Subfase 1.2.1 (EF Core 10 + Npgsql)
/// </summary>
public class InfrastructurePersistenceConfigurationTests
{
    [Fact]
    public void Infrastructure_Should_Reference_EntityFrameworkCore()
    {
        var hasEfCoreReference = HasPackageReference("Microsoft.EntityFrameworkCore");

        Assert.True(hasEfCoreReference,
            "BuildingBlocks.Infrastructure deve referenciar Microsoft.EntityFrameworkCore na Subfase 1.2.1");
    }

    [Fact]
    public void Infrastructure_Should_Reference_NpgsqlEfCoreProvider()
    {
        var hasNpgsqlProviderReference = HasPackageReference("Npgsql.EntityFrameworkCore.PostgreSQL");

        Assert.True(hasNpgsqlProviderReference,
            "BuildingBlocks.Infrastructure deve referenciar Npgsql.EntityFrameworkCore.PostgreSQL na Subfase 1.2.1");
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
}
