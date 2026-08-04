using System.Reflection;
using BuildingBlocks.Shared;
using FluentAssertions;
using NetArchTest.Rules;

namespace Tests.Architecture;

public sealed class TenantOnboardingArchitectureTests
{
    [Fact]
    public void OnboardingEndpoints_Should_ReturnResultTypes()
    {
        var identityAssembly = GetAssemblyByName("Modules.Identity");
        var expected = new[]
        {
            "SaveOnboardingDraftEndpoint",
            "GetOnboardingDraftEndpoint",
            "GetCnpjStatusEndpoint",
            "GetCepLookupEndpoint",
            "CreateTenantOnboardingEndpoint"
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
    public void Administradora_And_Condominio_Should_ResideIn_IdentityDomain()
    {
        var identityAssembly = GetAssemblyByName("Modules.Identity");

        var admin = identityAssembly.GetType("Modules.Identity.Domain.Administradora");
        var condo = identityAssembly.GetType("Modules.Identity.Domain.Condominio");

        admin.Should().NotBeNull();
        condo.Should().NotBeNull();
    }

    private static Assembly GetAssemblyByName(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            var currentDir = Path.GetDirectoryName(typeof(TenantOnboardingArchitectureTests).Assembly.Location);
            var assemblyPath = Directory.GetFiles(currentDir!, $"{assemblyName}.dll", SearchOption.AllDirectories).FirstOrDefault();
            return assemblyPath != null
                ? Assembly.LoadFrom(assemblyPath)
                : throw new FileNotFoundException($"Assembly {assemblyName} não encontrado.");
        }
    }
}
