using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Infrastructure.Persistence;
using Modules.Operations.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Operations;

public sealed class AreaComumIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_operations_test")
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

    private OperationsDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService
        {
            TenantId = tenantId,
            CondoId = condoId
        };

        var options = new DbContextOptionsBuilder<OperationsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new OperationsDbContext(options, tenantService);
    }

    [Fact]
    public async Task AreaComumService_Should_Create_And_Enforce_Tenant_Isolation()
    {
        // Arrange - Criar schema
        await using (var setupCtx = CreateDbContext(1))
        {
            await setupCtx.Database.EnsureCreatedAsync();
        }

        // Act 1: Tenant 1 cadastra "Salão de Festas Principal"
        await using (var ctxTenant1 = CreateDbContext(1))
        {
            var repo = new AreaComumRepository(ctxTenant1);
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var appService = new AreaComumApplicationService(repo, tenantService);

            var request = new CreateAreaComumRequest(
                CondoId: 1,
                Nome: "Salão de Festas Principal",
                Descricao: "Salão nobre do bloco A",
                Tipo: TipoAreaComum.Eventos,
                CapacidadeMaxima: 120,
                TaxaReserva: 180.00m,
                TaxaLimpeza: 60.00m,
                HorarioInicioFuncionamento: "08:00",
                HorarioFimFuncionamento: "22:00",
                TempoAntecedenciaMinimaDias: 2,
                TempoAntecedenciaMaximaDias: 60,
                RequerAprovacaoSindico: true,
                RegrasUso: "Som permitido até às 22h.");

            var createResult = await appService.CreateAsync(request);

            createResult.IsSuccess.Should().BeTrue();
            createResult.Data.Should().NotBeNull();
            createResult.Data!.CustoTotalReserva.Should().Be(240.00m);
            createResult.Data!.Status.Should().Be(StatusAreaComum.Ativa);
        }

        // Act 2: Tenant 2 tenta listar áreas comuns
        await using (var ctxTenant2 = CreateDbContext(2))
        {
            var repo = new AreaComumRepository(ctxTenant2);
            var tenantService = new TestCurrentTenantService { TenantId = 2, CondoId = 1 };
            var appService = new AreaComumApplicationService(repo, tenantService);

            var listResult = await appService.GetAllAsync(condoId: 1);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Data.Should().BeEmpty("Áreas comuns do Tenant 1 não devem ser visíveis para o Tenant 2.");
        }

        // Act 3: Tenant 1 consulta áreas comuns e atualiza status
        await using (var ctxTenant1 = CreateDbContext(1))
        {
            var repo = new AreaComumRepository(ctxTenant1);
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var appService = new AreaComumApplicationService(repo, tenantService);

            var listResult = await appService.GetAllAsync(condoId: 1);

            listResult.IsSuccess.Should().BeTrue();
            listResult.Data.Should().HaveCount(1);
            var area = listResult.Data!.First();
            area.Nome.Should().Be("Salão de Festas Principal");

            var statusResult = await appService.ChangeStatusAsync(area.Id, new ChangeAreaComumStatusRequest(StatusAreaComum.Manutencao));
            statusResult.IsSuccess.Should().BeTrue();
            statusResult.Data!.Status.Should().Be(StatusAreaComum.Manutencao);
        }
    }
}

internal class TestCurrentTenantService : ICurrentTenantService
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
