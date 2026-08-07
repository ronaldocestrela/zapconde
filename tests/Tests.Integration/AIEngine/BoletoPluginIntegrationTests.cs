using System.Text.Json;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AIEngine.Application.Plugins;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Tests.Integration.AIEngine;

public sealed class BoletoPluginIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_boleto_plugin_test")
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

    private FinancialDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService
        {
            TenantId = tenantId,
            CondoId = condoId
        };

        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new FinancialDbContext(options, tenantService);
    }

    [Fact]
    public async Task BoletoPlugin_DeveRetornarBoletosDoBancoRealEIsolarPorTenant()
    {
        // 1. Setup Banco de Dados
        await using (var db = CreateDbContext(tenantId: 1))
        {
            await db.Database.EnsureCreatedAsync();

            // Criar Fatura e Boleto no Tenant 1
            var faturaTenant1 = Fatura.Create(
                tenantId: 1,
                condoId: 1,
                unidadeId: 101,
                moradorId: 10,
                competencia: "2026-08",
                dataVencimento: DateTime.UtcNow.AddDays(7),
                observacoes: "Taxa Condominial Agosto/2026"
            );
            faturaTenant1.AddItem("Taxa Condominial Ordinária", Modules.Financial.Domain.Enums.TipoItemCobranca.TaxaCondominial, 350.00m, 1);

            var boletoTenant1 = Boleto.Create(
                tenantId: 1,
                faturaId: faturaTenant1.Id,
                nossoNumero: "34190123456",
                linhaDigitavel: "34191.79001 12345.67890",
                codigoBarras: "341981234567890",
                codigoPix: "00020126580014br.gov.bcb.pix0136zapcondo-pix-1-101",
                valor: 350.00m,
                dataVencimento: faturaTenant1.DataVencimento,
                pdfUrl: "/api/financial/invoices/1/pdf"
            );
            faturaTenant1.AnexarBoleto(boletoTenant1);

            db.Faturas.Add(faturaTenant1);
            await db.SaveChangesAsync();
        }

        // 2. Executa Plugin no contexto do Tenant 1
        await using (var dbTenant1 = CreateDbContext(tenantId: 1))
        {
            var tenantService1 = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var invoiceService1 = new InvoiceService(dbTenant1, tenantService1);
            var plugin1 = new BoletoPlugin(invoiceService1);

            var jsonResult = await plugin1.GetPendingBoletosAsync(10);
            jsonResult.Should().NotBeNullOrEmpty();

            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;
            root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
            root.GetProperty("totalPendencias").GetInt32().Should().Be(1);
            root.GetProperty("valorTotal").GetDecimal().Should().Be(350.00m);

            var boletos = root.GetProperty("boletos");
            boletos.GetArrayLength().Should().Be(1);
            boletos[0].GetProperty("pixCopiaECola").GetString().Should().Contain("zapcondo-pix-1-101");
        }

        // 3. Tenta consultar o mesmo morador no contexto do Tenant 2 (Garantia de Isolamento Multi-tenant)
        await using (var dbTenant2 = CreateDbContext(tenantId: 2))
        {
            var tenantService2 = new TestCurrentTenantService { TenantId = 2, CondoId = 1 };
            var invoiceService2 = new InvoiceService(dbTenant2, tenantService2);
            var plugin2 = new BoletoPlugin(invoiceService2);

            var jsonResultTenant2 = await plugin2.GetPendingBoletosAsync(10);

            using var doc2 = JsonDocument.Parse(jsonResultTenant2);
            var root2 = doc2.RootElement;
            root2.GetProperty("sucesso").GetBoolean().Should().BeTrue();
            root2.GetProperty("totalPendencias").GetInt32().Should().Be(0);
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
