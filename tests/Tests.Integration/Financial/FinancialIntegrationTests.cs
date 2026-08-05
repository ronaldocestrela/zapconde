using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Financial;

public sealed class FinancialIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_financial_test")
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
    public async Task InvoiceService_ShouldCreateAndQueryInvoices_WithTenantIsolation()
    {
        // Arrange - Setup Database schema
        await using (var seedContext = CreateDbContext(1))
        {
            await seedContext.Database.EnsureCreatedAsync();
        }

        // 1. Emissão de fatura no Tenant 1
        await using (var contextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var invoiceService = new InvoiceService(contextTenant1, tenantService);

            var createReq = new CreateFaturaRequest(
                CondoId: 10,
                UnidadeId: 101,
                MoradorId: 5,
                Competencia: "2026-08",
                DataVencimento: DateTime.UtcNow.AddDays(15),
                Observacoes: "Fatura de Teste Integração",
                Itens: new List<CreateItemCobrancaRequest>
                {
                    new("Taxa Condominial Ordinária", TipoItemCobranca.TaxaCondominial, 500.00m, 1),
                    new("Fundo de Reserva", TipoItemCobranca.FundoReserva, 50.00m, 1)
                }
            );

            var createRes = await invoiceService.CreateInvoiceAsync(createReq);
            createRes.IsSuccess.Should().BeTrue();
            createRes.Data.Should().NotBeNull();
            createRes.Data!.TotalFinal.Should().Be(550.00m);
            createRes.Data.Boleto.Should().NotBeNull();
            createRes.Data.Boleto!.LinhaDigitavel.Should().NotBeNullOrEmpty();
            createRes.Data.Boleto.CodigoPixCopiaECola.Should().Contain("zapcondo-pix-1");
        }

        // 2. Consulta no Tenant 1 deve retornar a fatura
        await using (var contextTenant1Query = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var invoiceService = new InvoiceService(contextTenant1Query, tenantService);

            var listRes = await invoiceService.GetInvoicesAsync(competencia: "2026-08");
            listRes.IsSuccess.Should().BeTrue();
            listRes.Data.Should().HaveCount(1);
            listRes.Data.First().TotalFinal.Should().Be(550.00m);
        }

        // 3. Consulta no Tenant 2 (isolamento) NÃO deve enxergar a fatura do Tenant 1
        await using (var contextTenant2Query = CreateDbContext(2))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 2, CondoId = 10 };
            var invoiceService = new InvoiceService(contextTenant2Query, tenantService);

            var listRes = await invoiceService.GetInvoicesAsync(competencia: "2026-08");
            listRes.IsSuccess.Should().BeTrue();
            listRes.Data.Should().BeEmpty("Global Query Filter deve impedir vazamento de faturas entre tenants.");
        }
    }

    [Fact]
    public async Task CancelInvoiceAsync_ShouldCancelPendingInvoice()
    {
        await using (var seedContext = CreateDbContext(1))
        {
            await seedContext.Database.EnsureCreatedAsync();
        }

        int invoiceId;
        await using (var context = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var invoiceService = new InvoiceService(context, tenantService);

            var createRes = await invoiceService.CreateInvoiceAsync(new CreateFaturaRequest(
                10, 102, 6, "2026-08", DateTime.UtcNow.AddDays(5), "Fatura a cancelar",
                new List<CreateItemCobrancaRequest> { new("Taxa", TipoItemCobranca.TaxaCondominial, 200m, 1) }
            ));

            invoiceId = createRes.Data!.Id;
        }

        // Cancelar fatura
        await using (var cancelContext = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var invoiceService = new InvoiceService(cancelContext, tenantService);

            var cancelRes = await invoiceService.CancelInvoiceAsync(invoiceId);
            cancelRes.IsSuccess.Should().BeTrue();

            var detailRes = await invoiceService.GetInvoiceByIdAsync(invoiceId);
            detailRes.IsSuccess.Should().BeTrue();
            detailRes.Data!.Status.Should().Be(StatusFatura.Cancelado);
            detailRes.Data.Boleto!.Status.Should().Be(StatusBoleto.Cancelado);
        }
    }

    private sealed class TestCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId { get; set; }
        public int? CondoId { get; set; }

        public void SetTenantId(int tenantId) => TenantId = tenantId;
        public void SetCondoId(int condoId) => CondoId = condoId;
        public void Clear() { TenantId = null; CondoId = null; }
    }
}
