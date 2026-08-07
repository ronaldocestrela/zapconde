using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Application.Services;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Infrastructure.Persistence;

namespace Tests.Unit.AIEngine;

public class AiOrchestratorServiceTests
{
    private readonly Mock<ICurrentTenantService> _tenantServiceMock;
    private readonly Mock<IAiKernelFactory> _kernelFactoryMock;
    private readonly DbContextOptions<AiDbContext> _dbOptions;

    public AiOrchestratorServiceTests()
    {
        _tenantServiceMock = new Mock<ICurrentTenantService>();
        _tenantServiceMock.Setup(t => t.TenantId).Returns(1);
        _tenantServiceMock.Setup(t => t.CondoId).Returns(1);

        _kernelFactoryMock = new Mock<IAiKernelFactory>();

        _dbOptions = new DbContextOptionsBuilder<AiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private AiDbContext CreateDbContext() => new(_dbOptions, _tenantServiceMock.Object);

    [Fact]
    public async Task SaveConfigAsync_ShouldPersistNewConfig_WhenNoConfigExists()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new AiOrchestratorService(context, _tenantServiceMock.Object, _kernelFactoryMock.Object);
        var command = new SaveAiConfigCommand(
            AiProvider.MockLocal,
            "gpt-4o-mini",
            "text-embedding-3-small",
            "",
            null,
            null,
            0.7,
            1500,
            true);

        // Act
        var result = await service.SaveConfigAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Provider.Should().Be(AiProvider.MockLocal);
        result.Data.ModelId.Should().Be("gpt-4o-mini");

        var dbConfig = await context.KernelConfigs.FirstOrDefaultAsync();
        dbConfig.Should().NotBeNull();
        dbConfig!.TenantId.Should().Be(1);
    }

    [Fact]
    public async Task ExecutePromptAsync_ShouldReturnMockResponse_WhenProviderIsMockLocal()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new AiOrchestratorService(context, _tenantServiceMock.Object, _kernelFactoryMock.Object);

        // Pre-cadastra configuração MockLocal
        var config = AiKernelConfig.Criar(1, 1, AiProvider.MockLocal, "gpt-4o-mini", "text-embedding-3-small", "");
        context.KernelConfigs.Add(config);
        await context.SaveChangesAsync();

        var request = new ExecutePromptRequestDto("Horário da piscina?");

        // Act
        var result = await service.ExecutePromptAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Response.Should().Contain("MockLocal");
        result.Data.Success.Should().BeTrue();

        var logs = await context.ExecutionLogs.ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Prompt.Should().Be("Horário da piscina?");
        logs[0].Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutePromptAsync_ShouldFail_WhenNoConfigActiveForTenant()
    {
        // Arrange
        using var context = CreateDbContext();
        var service = new AiOrchestratorService(context, _tenantServiceMock.Object, _kernelFactoryMock.Object);
        var request = new ExecutePromptRequestDto("Qual o regulamento?");

        // Act
        var result = await service.ExecutePromptAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Nenhuma configuração ativa");
    }
}
