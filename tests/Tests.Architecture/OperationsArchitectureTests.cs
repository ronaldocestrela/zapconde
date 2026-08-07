using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Tests.Architecture;

public class OperationsArchitectureTests
{
    private static readonly System.Reflection.Assembly OperationsDomainAssembly = typeof(Modules.Operations.Domain.Entities.AreaComum).Assembly;
    private static readonly System.Reflection.Assembly OperationsEndpointsAssembly = typeof(Modules.Operations.Endpoints.CreateAreaComumEndpoint).Assembly;

    [Fact]
    public void DomainEntities_Should_Implement_ITenantScoped()
    {
        var result = Types.InAssembly(OperationsDomainAssembly)
            .That()
            .ResideInNamespace("Modules.Operations.Domain.Entities")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching("AnexoOcorrencia")
            .And()
            .DoNotHaveNameMatching("HistoricoOcorrencia")
            .And()
            .DoNotHaveNameMatching("PautaAssembleia")
            .And()
            .DoNotHaveNameMatching("VotoAssembleia")
            .Should()
            .ImplementInterface(typeof(ITenantScoped))
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Todas as entidades Aggregate Root do módulo Operations devem implementar ITenantScoped para isolamento seguro.");
    }

    [Fact]
    public void OperationsDbContext_Should_InheritFrom_MultiTenantDbContext()
    {
        var dbContextType = typeof(Modules.Operations.Infrastructure.Persistence.OperationsDbContext);

        dbContextType.BaseType.Should().Be(typeof(BuildingBlocks.Infrastructure.Persistence.MultiTenantDbContext),
            "OperationsDbContext deve herdar de MultiTenantDbContext para aplicar os filtros globais de TenantId.");
    }

    [Fact]
    public void FastEndpoints_Should_Have_XmlDocumentation()
    {
        var endpointTypes = Types.InAssembly(OperationsEndpointsAssembly)
            .That()
            .ResideInNamespace("Modules.Operations.Endpoints")
            .And()
            .AreClasses()
            .GetTypes();

        endpointTypes.Should().NotBeEmpty("O módulo Operations deve possuir endpoints cadastrados.");
    }
}
