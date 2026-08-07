using BuildingBlocks.Shared.Events;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.WhatsApp.Application.Services;
using Modules.WhatsApp.Domain.Enums;
using Modules.WhatsApp.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Tests.Integration.WhatsApp;

public sealed class WhatsAppMessagingIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_whatsapp_outbox_test")
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

    [Fact]
    public async Task IngestEvolutionWebhook_Should_Write_To_OutboxMessage_Table_Atomically_With_WebhookLog()
    {
        // Arrange
        var services = new ServiceCollection();
        var tenantService = new TestCurrentTenantService { TenantId = 5, CondoId = 2 };
        services.AddSingleton<ICurrentTenantService>(tenantService);
        services.AddSingleton<IEvolutionPayloadParser, EvolutionPayloadParser>();
        services.AddLogging();

        var connectionString = _postgresContainer.GetConnectionString();
        services.AddDbContext<WhatsAppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<WhatsAppDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<WhatsAppApplicationService>();

        await using var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
            await db.Database.EnsureCreatedAsync();
        }

        // Act - Ingerir Webhook via WhatsAppApplicationService
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            var parser = scope.ServiceProvider.GetRequiredService<IEvolutionPayloadParser>();
            var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<WhatsAppApplicationService>>();

            var service = new WhatsAppApplicationService(db, tenantService, parser, publishEndpoint, logger);

            var payloadJson = """
            {
              "event": "messages.upsert",
              "instance": "condo-central",
              "data": {
                "key": {
                  "remoteJid": "5575988887777@s.whatsapp.net",
                  "fromMe": false,
                  "id": "MSG_OUTBOX_TEST_100"
                },
                "pushName": "Residente Outbox",
                "message": {
                  "conversation": "Dúvida sobre taxa de condomínio"
                },
                "messageType": "conversation",
                "messageTimestamp": 1723000000
              }
            }
            """;

            var result = await service.IngestEvolutionWebhookAsync(payloadJson);
            result.IsSuccess.Should().BeTrue();
            result.Data!.IsDuplicate.Should().BeFalse();
        }

        // Assert - Verificar se o WebhookLog e a mensagem no Outboxforam salvos no PostgreSQL
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WhatsAppDbContext>();

            var logs = await db.WebhookLogs.IgnoreQueryFilters().ToListAsync();
            logs.Should().HaveCount(1);
            logs[0].MessageId.Should().Be("MSG_OUTBOX_TEST_100");
            logs[0].TenantId.Should().Be(5);

            // Verificar se existeelementos salvos na tabela OutboxMessage do MassTransit
            var outboxMessageType = db.Model.GetEntityTypes()
                .FirstOrDefault(e => e.ClrType.Name.Contains("OutboxMessage"))
                ?.ClrType;

            outboxMessageType.Should().NotBeNull("a entidade OutboxMessage deve fazer parte do modelo EF Core");

            var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
                .MakeGenericMethod(outboxMessageType!);
            var outboxQueryable = (IQueryable)setMethod.Invoke(db, null)!;

            var outboxCount = 0;
            foreach (var item in outboxQueryable)
            {
                outboxCount++;
            }

            outboxCount.Should().BeGreaterThan(0, "o evento WhatsAppMessageReceivedEvent deve ser salvo na tabela OutboxMessage durante o commit transacional");
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
