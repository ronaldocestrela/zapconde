using BuildingBlocks.Shared;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace Tests.Architecture;

public class FinancialArchitectureTests
{
    private static readonly System.Reflection.Assembly FinancialAssembly = typeof(Modules.Financial.Domain.Entities.Fatura).Assembly;

    [Fact]
    public void Financial_DomainEntities_ShouldImplement_ITenantScoped()
    {
        // Arrange
        var result = Types.InAssembly(FinancialAssembly)
            .That()
            .ResideInNamespace("Modules.Financial.Domain.Entities")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(ITenantScoped))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue("Todas as entidades de domínio do módulo Financial devem implementar ITenantScoped.");
    }

    [Fact]
    public void Financial_Services_ShouldReturn_Result_Or_ResultT()
    {
        // Arrange & Act
        var serviceInterface = typeof(Modules.Financial.Application.Services.IInvoiceService);
        var methods = serviceInterface.GetMethods();

        // Assert
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
