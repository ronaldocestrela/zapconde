using BuildingBlocks.Infrastructure.Persistence;
using BuildingBlocks.Shared.MultiTenancy;

namespace Tests.Architecture;

/// <summary>
/// Testes de conformidade arquitetural da Subfase 1.3.2:
/// Transactional Outbox Pattern com EF Core e MassTransit.
/// </summary>
public class OutboxConfigurationArchitectureTests
{
    [Fact]
    public void Infrastructure_Should_Reference_MassTransitEntityFrameworkCore()
    {
        var hasMassTransitEfCoreReference = HasPackageReference("MassTransit.EntityFrameworkCore");

        Assert.True(hasMassTransitEfCoreReference,
            "BuildingBlocks.Infrastructure deve referenciar MassTransit.EntityFrameworkCore na Subfase 1.3.2");
    }

    [Fact]
    public void Outbox_Entities_In_Infrastructure_Model_Should_Not_Implement_ITenantScoped()
    {
        var outboxTypes = typeof(MultiTenantDbContext).Assembly.GetTypes()
            .Where(t => t.Name.Contains("Outbox") || t.Name.Contains("Inbox"));

        Assert.All(outboxTypes, t => Assert.False(typeof(ITenantScoped).IsAssignableFrom(t)));
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
