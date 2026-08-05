using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Financial;

public sealed class FinancialCalculationIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_financial_calc_test")
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
    public async Task FinancialCalculationService_SimulateAdHoc_ShouldReturnValidCalculations()
    {
        // Arrange
        await using var context = CreateDbContext(1);
        var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
        var calculadora = new CalculadoraFinanceira();
        var calcService = new FinancialCalculationService(context, tenantService, calculadora);

        var request = new SimularCalculoRequestDto(
            ValorOriginal: 1200.00m,
            DataVencimento: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            DataSimulacao: new DateTime(2026, 8, 16, 0, 0, 0, DateTimeKind.Utc), // 15 dias atraso
            PercentualMulta: 2.0m,
            PercentualJurosMensal: 1.0m
        );

        // Act
        var result = await calcService.CalcularSimulacaoAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.DiasAtraso.Should().Be(15);
        result.Data.ValorMulta.Should().Be(24.00m); // 2% de 1200
        result.Data.ValorJuros.Should().Be(6.00m);  // 1200 * (1%/30)*15 = 6.00
        result.Data.ValorTotalCalculado.Should().Be(1230.00m);
        result.Data.MemoriaCalculoTextual.Should().Contain("15 dia(s) corrido(s)");
    }

    [Fact]
    public async Task FinancialCalculationService_SimulateExistingInvoice_WithPostgres_ShouldWorkWithTenantIsolation()
    {
        // Arrange & Seed
        await using (var seedContext = CreateDbContext(1))
        {
            await seedContext.Database.EnsureCreatedAsync();
        }

        int invoiceId;
        await using (var contextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var invoiceService = new InvoiceService(contextTenant1, tenantService);

            var createRes = await invoiceService.CreateInvoiceAsync(new CreateFaturaRequest(
                10, 101, 5, "2026-08", new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), "Fatura para simulação",
                new List<CreateItemCobrancaRequest> { new("Taxa Condominial", TipoItemCobranca.TaxaCondominial, 600m, 1) }
            ));

            invoiceId = createRes.Data!.Id;
        }

        // Act 1: Simular no Tenant 1 (sucesso)
        await using (var simContextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var calculadora = new CalculadoraFinanceira();
            var calcService = new FinancialCalculationService(simContextTenant1, tenantService, calculadora);

            var simDate = new DateTime(2026, 8, 11, 0, 0, 0, DateTimeKind.Utc); // 10 dias
            var simRes = await calcService.SimularFaturaExistenteAsync(invoiceId, simDate, 1);

            simRes.IsSuccess.Should().BeTrue();
            simRes.Data!.DiasAtraso.Should().Be(10);
            simRes.Data.ValorMulta.Should().Be(12.00m); // 2% de 600
            simRes.Data.ValorJuros.Should().Be(2.00m);  // 600 * (1%/30)*10 = 2.00
            simRes.Data.ValorTotalCalculado.Should().Be(614.00m);
        }

        // Act 2: Obter Projeção Futura
        await using (var projContextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var calculadora = new CalculadoraFinanceira();
            var calcService = new FinancialCalculationService(projContextTenant1, tenantService, calculadora);

            var projRes = await calcService.ObterProjecaoFuturaAsync(invoiceId, 1);
            projRes.IsSuccess.Should().BeTrue();
            projRes.Data.Should().HaveCount(5); // 0, 7, 15, 30, 60 dias
        }

        // Act 3: Simular no Tenant 2 (falha por isolamento multi-tenant)
        await using (var simContextTenant2 = CreateDbContext(2))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 2, CondoId = 10 };
            var calculadora = new CalculadoraFinanceira();
            var calcService = new FinancialCalculationService(simContextTenant2, tenantService, calculadora);

            var simRes = await calcService.SimularFaturaExistenteAsync(invoiceId, DateTime.UtcNow, 2);
            simRes.IsSuccess.Should().BeFalse();
            simRes.Message.Should().Contain("não encontrada ou inacessível");
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
