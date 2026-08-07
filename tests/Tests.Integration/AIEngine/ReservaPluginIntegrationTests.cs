using System.Text.Json;
using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AIEngine.Application.Plugins;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Entities;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Infrastructure.Persistence;
using Modules.Operations.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;

namespace Tests.Integration.AIEngine;

public sealed class ReservaPluginIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_reserva_plugin_test")
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
    public async Task ReservaPlugin_DeveAgendarAreaComumNoBancoRealEIsolarPorTenant()
    {
        int areaIdTenant1;

        // 1. Setup Banco de Dados
        await using (var db = CreateDbContext(tenantId: 1))
        {
            await db.Database.EnsureCreatedAsync();

            var areaTenant1 = AreaComum.Create(
                tenantId: 1,
                condoId: 1,
                nome: "Salão de Festas Principal",
                descricao: "Salão de festas decorado",
                tipo: TipoAreaComum.Eventos,
                capacidadeMaxima: 60,
                taxaReserva: 100.00m,
                taxaLimpeza: 50.00m,
                horarioInicioFuncionamento: TimeSpan.FromHours(8),
                horarioFimFuncionamento: TimeSpan.FromHours(23),
                tempoAntecedenciaMinimaDias: 0,
                tempoAntecedenciaMaximaDias: 60,
                requerAprovacaoSindico: true
            );

            db.AreasComuns.Add(areaTenant1);
            await db.SaveChangesAsync();
            areaIdTenant1 = areaTenant1.Id;
        }

        // 2. Executa Plugin no contexto do Tenant 1
        await using (var dbTenant1 = CreateDbContext(tenantId: 1))
        {
            var tenantService1 = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var lockService = new InMemoryDistributedLockService();
            var reservaRepo = new ReservaRepository(dbTenant1);
            var areaRepo = new AreaComumRepository(dbTenant1);

            var reservaAppService = new ReservaApplicationService(reservaRepo, areaRepo, lockService, tenantService1);
            var areaAppService = new AreaComumApplicationService(areaRepo, tenantService1);

            var plugin1 = new ReservaPlugin(reservaAppService, areaAppService, tenantService1);

            var dataInicio = DateTime.UtcNow.AddDays(2).Date.AddHours(18).ToString("yyyy-MM-dd HH:mm");
            var dataFim = DateTime.UtcNow.AddDays(2).Date.AddHours(22).ToString("yyyy-MM-dd HH:mm");

            var jsonResult = await plugin1.ReserveCommonAreaAsync(
                areaId: areaIdTenant1,
                dataInicio: dataInicio,
                dataFim: dataFim,
                moradorId: 10,
                quantidadePessoas: 30,
                observacao: "Aniversário 30 Anos");

            jsonResult.Should().NotBeNullOrEmpty();

            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
            root.GetProperty("nomeAreaComum").GetString().Should().Be("Salão de Festas Principal");
            root.GetProperty("status").GetString().Should().Be("PendenteAprovacao");
            root.GetProperty("valorTotal").GetDecimal().Should().Be(150.00m);
        }

        // 3. Tenta consultar/agendar a área do Tenant 1 a partir do Tenant 2 (Garantia de Isolamento Multi-tenant)
        await using (var dbTenant2 = CreateDbContext(tenantId: 2))
        {
            var tenantService2 = new TestCurrentTenantService { TenantId = 2, CondoId = 1 };
            var lockService = new InMemoryDistributedLockService();
            var reservaRepo2 = new ReservaRepository(dbTenant2);
            var areaRepo2 = new AreaComumRepository(dbTenant2);

            var reservaAppService2 = new ReservaApplicationService(reservaRepo2, areaRepo2, lockService, tenantService2);
            var areaAppService2 = new AreaComumApplicationService(areaRepo2, tenantService2);

            var plugin2 = new ReservaPlugin(reservaAppService2, areaAppService2, tenantService2);

            var dataInicio = DateTime.UtcNow.AddDays(2).Date.AddHours(18).ToString("yyyy-MM-dd HH:mm");
            var dataFim = DateTime.UtcNow.AddDays(2).Date.AddHours(22).ToString("yyyy-MM-dd HH:mm");

            var jsonResultTenant2 = await plugin2.ReserveCommonAreaAsync(
                areaId: areaIdTenant1,
                dataInicio: dataInicio,
                dataFim: dataFim,
                moradorId: 10);

            using var doc2 = JsonDocument.Parse(jsonResultTenant2);
            var root2 = doc2.RootElement;

            root2.GetProperty("sucesso").GetBoolean().Should().BeFalse();
            root2.GetProperty("mensagem").GetString().Should().Contain("não encontrada");
        }
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
