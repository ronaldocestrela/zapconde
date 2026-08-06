using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Tests.Architecture;

public class FinancialAgreementArchitectureTests
{
    private static readonly System.Reflection.Assembly FinancialAssembly = typeof(Modules.Financial.Domain.Entities.Acordo).Assembly;

    [Fact]
    public void AgreementAndDunning_DomainEntities_ShouldImplement_ITenantScoped()
    {
        var result = Types.InAssembly(FinancialAssembly)
            .That()
            .ResideInNamespace("Modules.Financial.Domain.Entities")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(ITenantScoped))
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Todas as entidades de domínio de Acordo e Régua devem implementar ITenantScoped.");
    }

    [Fact]
    public void AgreementAndDunning_AppServices_ShouldReturn_Result_Or_ResultT()
    {
        var interfaces = new[]
        {
            typeof(Modules.Financial.Application.Services.IAcordoApplicationService),
            typeof(Modules.Financial.Application.Services.IReguaInadimplenciaAppService)
        };

        foreach (var serviceInterface in interfaces)
        {
            var methods = serviceInterface.GetMethods();

            foreach (var method in methods)
            {
                var returnType = method.ReturnType;
                var isGenericResult = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>) &&
                                      returnType.GetGenericArguments()[0].IsGenericType &&
                                      returnType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Result<>);

                var isNonGenericResult = returnType == typeof(Task<Result>);

                (isGenericResult || isNonGenericResult).Should().BeTrue(
                    $"O método {method.Name} da interface {serviceInterface.Name} deve retornar Task<Result> ou Task<Result<T>>.");
            }
        }
    }
}
