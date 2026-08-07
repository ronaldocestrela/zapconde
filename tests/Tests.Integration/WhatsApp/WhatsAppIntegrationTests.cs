using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Modules.WhatsApp.Application.DTOs;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.WhatsApp;

public sealed class WhatsAppIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_whatsapp_test")
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

    private WhatsAppDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService
        {
            TenantId = tenantId,
            CondoId = condoId
        };

        var options = new DbContextOptionsBuilder<WhatsAppDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new WhatsAppDbContext(options, tenantService);
    }

    [Fact]
    public async Task FluxoCompleto_IngerirWebhookEInstancia_DeveGravarEFiltrarNoPostgres()
    {
        // 1. Setup Database
        await using (var db = CreateDbContext(tenantId: 10))
        {
            await db.Database.EnsureCreatedAsync();
        }

        var tenantService = new TestCurrentTenantService { TenantId = 10, CondoId = 1 };
        await using var dbContext = CreateDbContext(tenantId: 10);
        var parser = new EvolutionPayloadParser();
        var logger = NullLogger<WhatsAppApplicationService>.Instance;
        var service = new WhatsAppApplicationService(dbContext, tenantService, parser, logger);

        // 2. Cadastrar Instância
        var createCmd = new CreateWhatsAppInstanceCommand(
            CondoId: 1,
            InstanceName: "condo-central",
            Provider: "EvolutionApi",
            BaseUrl: "https://api.evolution.com",
            ApiKey: "KEY_123",
            WebhookSecret: null
        );

        var instanceResult = await service.CreateInstanceAsync(createCmd);
        instanceResult.IsSuccess.Should().BeTrue();
        instanceResult.Data.Should().NotBeNull();
        instanceResult.Data!.InstanceName.Should().Be("condo-central");

        // 3. Ingerir Webhook da Evolution API
        var payloadJson = """
        {
          "event": "messages.upsert",
          "instance": "condo-central",
          "data": {
            "key": {
              "remoteJid": "5575999999999@s.whatsapp.net",
              "fromMe": false,
              "id": "MSG_INTEGRATION_001"
            },
            "pushName": "Morador Teste",
            "message": {
              "conversation": "Olá, preciso emitir a 2 via do boleto"
            },
            "messageType": "conversation",
            "messageTimestamp": 1723000000
          }
        }
        """;

        var webhookResult = await service.IngestEvolutionWebhookAsync(payloadJson, "KEY_123");
        webhookResult.IsSuccess.Should().BeTrue();
        webhookResult.Data!.IsDuplicate.Should().BeFalse();

        // 4. Testar Idempotência com Payload Duplicado
        var duplicateResult = await service.IngestEvolutionWebhookAsync(payloadJson, "KEY_123");
        duplicateResult.IsSuccess.Should().BeTrue();
        duplicateResult.Data!.IsDuplicate.Should().BeTrue();

        // 5. Consultar Logs no Banco
        var logsResult = await service.GetWebhookLogsAsync();
        logsResult.IsSuccess.Should().BeTrue();
        logsResult.Data.Should().HaveCount(1);
        var logDto = logsResult.Data!.First();
        logDto.SenderPhone.Should().Be("+5575999999999");
        logDto.MessageText.Should().Be("Olá, preciso emitir a 2 via do boleto");
        logDto.Status.Should().Be("Received");

        // 6. Consultar Resumo KPI
        var summaryResult = await service.GetSummaryAsync();
        summaryResult.IsSuccess.Should().BeTrue();
        summaryResult.Data!.TotalRecebidosHoje.Should().Be(1);
        summaryResult.Data.InstanciasAtivas.Should().Be(1);
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
