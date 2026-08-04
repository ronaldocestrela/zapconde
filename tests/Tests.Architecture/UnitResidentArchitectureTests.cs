using System.Reflection;
using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using NetArchTest.Rules;

namespace Tests.Architecture;

public sealed class UnitResidentArchitectureTests
{
    [Fact]
    public void UnitEndpoints_Should_ReturnResultTypes()
    {
        var identityAssembly = GetAssemblyByName("Modules.Identity");
        var expected = new[]
        {
            "GetBlocksEndpoint",
            "CreateBlockEndpoint",
            "GetUnitsEndpoint",
            "CreateUnitEndpoint",
            "UpdateUnitEndpoint",
            "TransferOwnershipEndpoint",
            "GetUnitHistoryEndpoint",
            "PreviewUnitImportEndpoint",
            "CommitUnitImportEndpoint"
        };

        foreach (var name in expected)
        {
            var endpoint = identityAssembly.GetType($"Modules.Identity.Endpoints.{name}");
            endpoint.Should().NotBeNull(name);
            var baseType = endpoint!.BaseType;
            baseType.Should().NotBeNull();
            baseType!.IsGenericType.Should().BeTrue();
            var responseType = baseType.GetGenericArguments().Last();
            responseType.Name.Should().StartWith("Result");
        }
    }

    [Fact]
    public void UnitDomainEntities_Should_ImplementITenantScoped()
    {
        var identityAssembly = GetAssemblyByName("Modules.Identity");
        var types = new[] { "Bloco", "Unidade", "Morador", "VinculoUnidade" };

        foreach (var typeName in types)
        {
            var type = identityAssembly.GetType($"Modules.Identity.Domain.{typeName}");
            type.Should().NotBeNull(typeName);
            type!.GetInterfaces().Should().Contain(i => i == typeof(ITenantScoped));
        }
    }

    [Fact]
    public void UnitResidentService_Should_BeRegisteredInIdentityModule()
    {
        var identityAssembly = GetAssemblyByName("Modules.Identity");
        var service = identityAssembly.GetType("Modules.Identity.Infrastructure.Services.UnitResidentService");
        var iface = identityAssembly.GetType("Modules.Identity.Application.IUnitResidentService");

        service.Should().NotBeNull();
        iface.Should().NotBeNull();
        iface!.IsAssignableFrom(service!).Should().BeTrue();
    }

    private static Assembly GetAssemblyByName(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            var currentDir = Path.GetDirectoryName(typeof(UnitResidentArchitectureTests).Assembly.Location);
            var assemblyPath = Directory.GetFiles(currentDir!, $"{assemblyName}.dll", SearchOption.AllDirectories).FirstOrDefault();
            return assemblyPath != null
                ? Assembly.LoadFrom(assemblyPath)
                : throw new FileNotFoundException($"Assembly {assemblyName} não encontrado.");
        }
    }
}
