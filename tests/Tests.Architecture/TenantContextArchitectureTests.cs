using NetArchTest.Rules;

namespace Tests.Architecture;

public class TenantContextArchitectureTests
{
    [Fact]
    public void TenantHttpHeaders_Should_Exist_In_BuildingBlocksShared()
    {
        var sharedAssembly = GetAssemblyByName("BuildingBlocks.Shared");

        var result = Types.InAssembly(sharedAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Shared.MultiTenancy")
            .And()
            .HaveNameMatching("TenantHttpHeaders")
            .GetTypes();

        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public void TenantContextMiddleware_Should_Exist_In_BuildingBlocksInfrastructure()
    {
        var infrastructureAssembly = GetAssemblyByName("BuildingBlocks.Infrastructure");

        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace("BuildingBlocks.Infrastructure.MultiTenancy")
            .And()
            .AreClasses()
            .And()
            .HaveName("TenantContextMiddleware")
            .GetTypes();

        Assert.NotEmpty(result);
        Assert.Single(result);
    }

    [Fact]
    public void ICurrentTenantService_Should_Expose_SetAndClearMethods()
    {
        var sharedAssembly = GetAssemblyByName("BuildingBlocks.Shared");
        var serviceType = sharedAssembly
            .GetTypes()
            .Single(t => t.Name == "ICurrentTenantService");

        Assert.NotNull(serviceType.GetMethod("SetTenantId"));
        Assert.NotNull(serviceType.GetMethod("SetCondoId"));
        Assert.NotNull(serviceType.GetMethod("Clear"));
        Assert.NotNull(serviceType.GetProperty("CondoId"));
    }

    private static System.Reflection.Assembly GetAssemblyByName(string assemblyName)
    {
        try
        {
            return System.Reflection.Assembly.Load(assemblyName);
        }
        catch
        {
            var currentDir = Path.GetDirectoryName(typeof(TenantContextArchitectureTests).Assembly.Location);
            var searchPattern = $"{assemblyName}.dll";
            var assemblyPath = Directory.GetFiles(currentDir!, searchPattern, SearchOption.AllDirectories).FirstOrDefault();

            return assemblyPath != null
                ? System.Reflection.Assembly.LoadFrom(assemblyPath)
                : throw new FileNotFoundException($"Assembly {assemblyName} não encontrado.");
        }
    }
}
