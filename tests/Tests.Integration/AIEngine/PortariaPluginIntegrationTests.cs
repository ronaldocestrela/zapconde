using System.Text.Json;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Infrastructure.Persistence;
using Modules.AIEngine.Application.Plugins;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.AIEngine;

public sealed class PortariaPluginIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_portaria_plugin_test")
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

    private AccessControlDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService
        {
            TenantId = tenantId,
            CondoId = condoId
        };

        var options = new DbContextOptionsBuilder<AccessControlDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new AccessControlDbContext(options, tenantService);
    }

    [Fact]
    public async Task PortariaPlugin_DeveRegistrarAutorizacaoNoBancoRealEIsolarPorTenant()
    {
        // 1. Setup Banco de Dados
        await using (var db = CreateDbContext(tenantId: 1))
        {
            await db.Database.EnsureCreatedAsync();
        }

        int autorizacaoIdTenant1;

        // 2. Executa Plugin no contexto do Tenant 1
        await using (var dbTenant1 = CreateDbContext(tenantId: 1))
        {
            var tenantService1 = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var visitanteService1 = new VisitanteApplicationService(dbTenant1, tenantService1);
            var plugin1 = new PortariaPlugin(visitanteService1, tenantService1);

            var jsonResult = await plugin1.AuthorizeGuestAsync(
                nome: "Carlos Eduardo",
                documento: "123.456.789-00",
                dataInicio: "2026-09-20 14:00",
                dataFim: "2026-09-20 18:00",
                tipo: "Visitante",
                unidadeId: 102,
                blocoUnidade: "Bloco A - Apto 102",
                moradorId: 10,
                telefone: "+5575988887777",
                placaVeiculo: "ABC-1234",
                observacoes: "Visita familiar");

            jsonResult.Should().NotBeNullOrEmpty();

            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
            root.GetProperty("nomeCompleto").GetString().Should().Be("Carlos Eduardo");
            root.GetProperty("documento").GetString().Should().Be("123.456.789-00");
            root.GetProperty("status").GetString().Should().Be("Agendado");

            autorizacaoIdTenant1 = root.GetProperty("autorizacaoId").GetInt32();
            autorizacaoIdTenant1.Should().BeGreaterThan(0);
        }

        // 3. Tenta consultar a autorização a partir do Tenant 2 (Garantia de Isolamento Multi-tenant via Global Query Filter)
        await using (var dbTenant2 = CreateDbContext(tenantId: 2))
        {
            var visitanteTenant2 = await dbTenant2.Visitantes
                .FirstOrDefaultAsync(v => v.Id == autorizacaoIdTenant1);

            visitanteTenant2.Should().BeNull("O Global Query Filter por tenant_id deve impedir que o Tenant 2 visualize registros do Tenant 1");
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
