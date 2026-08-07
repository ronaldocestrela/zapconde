using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Operations;

public sealed class PlanoManutencaoIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_maintenance_test")
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
    public async Task PlanoManutencaoService_Should_Create_List_Conclude_And_Enforce_Tenant_Isolation()
    {
        // 1. Setup Database Schema
        using (var setupContext = CreateDbContext(tenantId: 1, condoId: 1))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        // 2. Tenant 1 Creates a Maintenance Plan
        var tenant1Service = new TestCurrentTenantService { TenantId = 1, CondoId = 100 };
        using (var tenant1Context = CreateDbContext(tenant1Service.TenantId, tenant1Service.CondoId))
        {
            var service = new PlanoManutencaoApplicationService(tenant1Context, tenant1Service);
            var createRequest = new CreatePlanoManutencaoRequest(
                CondoId: 100,
                Titulo: "Manutenção Preventiva de Elevador Social",
                Categoria: CategoriaManutencao.Elevadores,
                Periodicidade: PeriodicidadeManutencao.Mensal,
                DataProximaManutencao: DateTime.Today.AddDays(10),
                Descricao: "Vistoria mensal nos cabos de aço",
                ResponsavelTecnico: "Eng. Lucas",
                EmpresaContratada: "Elevadores Otis",
                CustoEstimado: 1200.00m
            );

            var createResult = await service.CriarPlanoAsync(createRequest);
            createResult.IsSuccess.Should().BeTrue(createResult.Message);
            createResult.Data.Should().NotBeNull();
            createResult.Data!.Titulo.Should().Be("Manutenção Preventiva de Elevador Social");
            createResult.Data!.Status.Should().Be(StatusManutencao.Proxima);

            var planoId = createResult.Data!.Id;

            // 3. Conclude Maintenance
            var completeRequest = new ConcluirManutencaoRequest(
                DataRealizacao: DateTime.Today,
                CustoReal: 1150.00m,
                Observacoes: "Troca do lubrificante de trilho realizada.",
                AgendarProxima: true
            );

            var completeResult = await service.ConcluirManutencaoAsync(planoId, completeRequest);
            completeResult.IsSuccess.Should().BeTrue();
            completeResult.Data!.Status.Should().Be(StatusManutencao.EmDia);
            completeResult.Data!.CustoReal.Should().Be(1150.00m);
            completeResult.Data!.DataProximaManutencao.Should().Be(DateTime.Today.AddMonths(1));

            // 4. Verify Summary KPI
            var summaryResult = await service.ObterResumoMetricasAsync(100);
            summaryResult.IsSuccess.Should().BeTrue();
            summaryResult.Data!.Total.Should().Be(1);
            summaryResult.Data!.EmDia.Should().Be(1);
            summaryResult.Data!.TotalCustoReal.Should().Be(1150.00m);
        }

        // 5. Tenant 2 Isolation Check
        var tenant2Service = new TestCurrentTenantService { TenantId = 2, CondoId = 100 };
        using (var tenant2Context = CreateDbContext(tenant2Service.TenantId, tenant2Service.CondoId))
        {
            var serviceTenant2 = new PlanoManutencaoApplicationService(tenant2Context, tenant2Service);

            var listTenant2 = await serviceTenant2.ListarAsync(condoId: 100);
            listTenant2.IsSuccess.Should().BeTrue();
            listTenant2.Data.Should().BeEmpty(); // Isolation enforced by Global Query Filter
        }
    }
}
