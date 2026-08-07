using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Application.Services;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Tests.Integration.AIEngine;

public sealed class AiIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_ai_test")
        .WithUsername("smartcondo")
        .WithPassword("smartcondo")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    private AiDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService
        {
            TenantId = tenantId,
            CondoId = condoId
        };

        var options = new DbContextOptionsBuilder<AiDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new AiDbContext(options, tenantService);
    }

    [Fact]
    public async Task FluxoCompleto_SalvarConfiguracaoEExecutarPrompt_DevePersistirEFiltrarNoPostgres()
    {
        // 1. Setup Database e Migrações
        await using (var db = CreateDbContext(tenantId: 1))
        {
            await db.Database.EnsureCreatedAsync();
        }

        await using var dbContext = CreateDbContext(tenantId: 1);
        var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
        var factory = new AiKernelFactory();
        var service = new AiOrchestratorService(dbContext, tenantService, factory);

        // 2. Salva Configuração MockLocal
        var saveCommand = new SaveAiConfigCommand(
            AiProvider.MockLocal,
            "gpt-4o-mini",
            "text-embedding-3-small",
            "",
            null,
            null,
            0.7,
            1000,
            true);

        var configResult = await service.SaveConfigAsync(saveCommand);
        configResult.IsSuccess.Should().BeTrue();

        // 3. Executa Prompt
        var executeResult = await service.ExecutePromptAsync(new ExecutePromptRequestDto("Qual o horário da portaria?"));
        executeResult.IsSuccess.Should().BeTrue();
        executeResult.Data.Should().NotBeNull();
        executeResult.Data!.Response.Should().Contain("MockLocal");

        // 4. Valida Resumo e Logs
        var summaryResult = await service.GetSummaryAsync();
        summaryResult.IsSuccess.Should().BeTrue();
        summaryResult.Data!.TotalExecucoes.Should().Be(1);
        summaryResult.Data.ExecucoesComSucesso.Should().Be(1);

        var logsResult = await service.GetLogsAsync();
        logsResult.IsSuccess.Should().BeTrue();
        logsResult.Data.Should().HaveCount(1);
    }

    private sealed class TestCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId { get; set; }
        public int? CondoId { get; set; }
        public int? UserId { get; set; }
        public string? UserRole { get; set; }

        public void SetTenantId(int tenantId) => TenantId = tenantId;
        public void SetCondoId(int condoId) => CondoId = condoId;
        public void Clear()
        {
            TenantId = null;
            CondoId = null;
            UserId = null;
            UserRole = null;
        }
    }
}
