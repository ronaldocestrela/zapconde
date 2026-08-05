using BuildingBlocks.Infrastructure.Caching;
using BuildingBlocks.Shared.Caching;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Infrastructure.Persistence;
using Modules.Financial.Infrastructure.Services;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Financial;

public sealed class PaymentGatewayIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_gateway_test")
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
    public async Task GeneratePayment_And_ProcessWebhookIdempotently_ShouldWorkEndToEnd()
    {
        // 1. Setup DB Schema
        await using (var schemaContext = CreateDbContext(1))
        {
            await schemaContext.Database.EnsureCreatedAsync();
        }

        int faturaId;

        // 2. Create Invoice
        await using (var contextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var invoiceService = new InvoiceService(contextTenant1, tenantService);

            var createRes = await invoiceService.CreateInvoiceAsync(new CreateFaturaRequest(
                10, 101, 5, "2026-08", new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc), "Fatura Gateway Test",
                new List<CreateItemCobrancaRequest> { new("Taxa Condominial", TipoItemCobranca.TaxaCondominial, 450m, 1) }
            ));

            createRes.IsSuccess.Should().BeTrue();
            faturaId = createRes.Data!.Id;
        }

        // 3. Generate Payment via Gateway Application Service
        string externalChargeId;
        await using (var genContext = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var mockGateway = new MockPaymentGatewayService();
            var paymentAppService = new InvoicePaymentApplicationService(
                genContext, mockGateway, tenantService, NullLogger<InvoicePaymentApplicationService>.Instance);

            var genRes = await paymentAppService.GeneratePaymentAsync(faturaId);
            genRes.IsSuccess.Should().BeTrue();
            genRes.Data.Should().NotBeNull();
            genRes.Data!.CodigoPixCopiaECola.Should().Contain("BR.GOV.BCB.PIX");
            genRes.Data.PixQrCodeBase64.Should().StartWith("data:image/svg+xml;base64,");

            externalChargeId = genRes.Data.ExternalChargeId;
            externalChargeId.Should().NotBeNullOrWhiteSpace();
        }

        // 4. Process Webhook PAYMENT_RECEIVED with Idempotency
        await using (var webhookContext = CreateDbContext(1))
        {
            var cache = new InMemoryCacheService(); // Implem in-memory de ICacheService para testes
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Financial:Asaas:WebhookToken", "secret-token-test" }
                })
                .Build();

            var webhookService = new PaymentWebhookService(
                webhookContext, cache, config, NullLogger<PaymentWebhookService>.Instance);

            var payload = new AsaasWebhookEventDto
            {
                Id = "evt_test_001",
                Event = "PAYMENT_RECEIVED",
                Payment = new AsaasPaymentDetailsDto
                {
                    Id = externalChargeId,
                    Value = 450m,
                    Status = "RECEIVED",
                    PaymentDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            };

            // Primeiras chamadas: Sucesso
            var webhookRes1 = await webhookService.ProcessAsaasWebhookAsync(payload, "secret-token-test");
            webhookRes1.IsSuccess.Should().BeTrue();
            webhookRes1.Data.Should().Contain("processado e fatura conciliada");

            // Segunda chamada com mesmo EventId: Idempotente
            var webhookRes2 = await webhookService.ProcessAsaasWebhookAsync(payload, "secret-token-test");
            webhookRes2.IsSuccess.Should().BeTrue();
            webhookRes2.Data.Should().Contain("idempotente");
        }

        // 5. Verify Invoice and Boleto Status updated to Pago
        await using (var verifyContext = CreateDbContext(1))
        {
            var fatura = await verifyContext.Faturas
                .Include(f => f.Boleto)
                .FirstOrDefaultAsync(f => f.Id == faturaId);

            fatura.Should().NotBeNull();
            fatura!.Status.Should().Be(StatusFatura.Pago);
            fatura.Boleto.Should().NotBeNull();
            fatura.Boleto!.Status.Should().Be(StatusBoleto.Pago);
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
