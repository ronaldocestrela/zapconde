using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Modules.Financial.Domain.Entities;
using Xunit;

namespace Tests.Architecture;

public class FinancialDigitalBinderArchitectureTests
{
    [Fact]
    public void FinancialDigitalBinderEntities_Should_Implement_ITenantScoped()
    {
        // Arrange
        var entityTypes = new[]
        {
            typeof(PastaDigital),
            typeof(DocumentoPrestacaoContas),
            typeof(ItemBalancete),
            typeof(ContaBancaria),
            typeof(ExtratoBancarioItem),
            typeof(ConciliacaoBancariaRecord)
        };

        // Act & Assert
        foreach (var type in entityTypes)
        {
            type.Should().Implement<ITenantScoped>();
        }
    }
}
