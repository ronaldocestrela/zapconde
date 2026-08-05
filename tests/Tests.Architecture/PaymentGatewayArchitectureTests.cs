using BuildingBlocks.Shared;
using FluentAssertions;
using Xunit;

namespace Tests.Architecture;

public class PaymentGatewayArchitectureTests
{
    [Fact]
    public void IPaymentGatewayService_Methods_ShouldReturn_TaskOfResultT()
    {
        // Arrange
        var serviceInterface = typeof(Modules.Financial.Application.Services.IPaymentGatewayService);
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
    public void IPaymentWebhookService_Methods_ShouldReturn_TaskOfResultT()
    {
        // Arrange
        var webhookInterface = typeof(Modules.Financial.Application.Services.IPaymentWebhookService);
        var methods = webhookInterface.GetMethods();

        // Assert
        foreach (var method in methods)
        {
            var returnType = method.ReturnType;
            var isGenericResult = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>) &&
                                  returnType.GetGenericArguments()[0].IsGenericType &&
                                  returnType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Result<>);

            isGenericResult.Should().BeTrue(
                $"O método {method.Name} da interface {webhookInterface.Name} deve retornar obrigatoriamente Task<Result<T>> para conformidade com o Result Pattern.");
        }
    }
}
