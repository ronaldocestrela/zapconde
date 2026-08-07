using FluentAssertions;
using Modules.AIEngine.Application.Services;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Domain.Exceptions;

namespace Tests.Unit.AIEngine;

public class AiKernelFactoryTests
{
    private readonly AiKernelFactory _factory = new();

    [Fact]
    public void CreateKernel_ShouldThrowException_WhenConfigIsNull()
    {
        // Act
        Action act = () => _factory.CreateKernel(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreateKernel_ShouldThrowException_WhenConfigIsInactive()
    {
        // Arrange
        var config = AiKernelConfig.Criar(1, 1, AiProvider.MockLocal, "gpt-4o-mini", "text-embedding-3-small", "");
        config.AlternarAtivo(); // Deativa

        // Act
        Action act = () => _factory.CreateKernel(config);

        // Assert
        act.Should().Throw<AiEngineDomainException>()
            .WithMessage("*inativa*");
    }

    [Fact]
    public void CreateKernel_ShouldReturnKernel_WhenProviderIsMockLocal()
    {
        // Arrange
        var config = AiKernelConfig.Criar(1, 1, AiProvider.MockLocal, "gpt-4o-mini", "text-embedding-3-small", "");

        // Act
        var kernel = _factory.CreateKernel(config);

        // Assert
        kernel.Should().NotBeNull();
    }

    [Fact]
    public void CreateKernel_ShouldReturnKernel_WhenProviderIsOpenAIWithValidKey()
    {
        // Arrange
        var config = AiKernelConfig.Criar(1, 1, AiProvider.OpenAI, "gpt-4o-mini", "text-embedding-3-small", "sk-proj-testkey12345");

        // Act
        var kernel = _factory.CreateKernel(config);

        // Assert
        kernel.Should().NotBeNull();
    }
}
