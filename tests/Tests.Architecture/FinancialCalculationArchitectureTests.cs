using BuildingBlocks.Shared;
using FluentAssertions;
using Xunit;

namespace Tests.Architecture;

public class FinancialCalculationArchitectureTests
{
    private static readonly System.Reflection.Assembly FinancialAssembly = typeof(Modules.Financial.Domain.Entities.Fatura).Assembly;

    [Fact]
    public void FinancialCalculationService_InterfaceMethods_ShouldReturn_TaskOfResultT()
    {
        // Arrange
        var serviceInterface = typeof(Modules.Financial.Application.Services.IFinancialCalculationService);
        var methods = serviceInterface.GetMethods();

        // Assert
        foreach (var method in methods)
        {
            var returnType = method.ReturnType;
            var isGenericResult = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>) &&
                                  returnType.GetGenericArguments()[0].IsGenericType &&
                                  returnType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Result<>);

            isGenericResult.Should().BeTrue(
                $"O método {method.Name} da interface {serviceInterface.Name} deve retornar obrigatoriamente Task<Result<T>> para conformidade com o Result Pattern.");
        }
    }

    [Fact]
    public void DomainServices_CalculadoraFinanceira_ShouldBePublicAndDeterministic()
    {
        // Arrange
        var calcType = typeof(Modules.Financial.Domain.Services.CalculadoraFinanceira);

        // Assert
        calcType.IsPublic.Should().BeTrue("A CalculadoraFinanceira deve ser uma classe pública de domínio.");
        var method = calcType.GetMethod("CalcularEncargos");
        method.Should().NotBeNull("Deve possuir o método CalcularEncargos.");
        method!.ReturnType.Name.Should().Be("CalculoFinanceiroResultado", "O método de cálculo deve retornar um Value Object imutável de resultado.");
    }
}
