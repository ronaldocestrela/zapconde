using System.Text.Json;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Infrastructure.Persistence;
using Modules.AIEngine.Application.Plugins;
using Modules.AIEngine.Application.Services;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.AIEngine;

public sealed class PackageVisionIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_package_vision_test")
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
    public async Task PackageVisionPlugin_DeveRegistrarEncomendaNoBancoRealEIsolarPorTenant()
    {
        // 1. Setup DB Schema
        await using (var db = CreateDbContext(tenantId: 1))
        {
            await db.Database.EnsureCreatedAsync();
        }

        int encomendaIdTenant1;

        // 2. Executa no Tenant 1
        await using (var dbTenant1 = CreateDbContext(tenantId: 1))
        {
            var tenantService1 = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var encomendaService1 = new EncomendaApplicationService(dbTenant1, tenantService1);

            var aiOrchestratorMock = new Mock<IAiOrchestratorService>();
            aiOrchestratorMock
                .Setup(a => a.ExecutePromptAsync(It.IsAny<Modules.AIEngine.Application.DTOs.ExecutePromptRequestDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildingBlocks.Shared.Result<Modules.AIEngine.Application.DTOs.ExecutePromptResponseDto>.Success(
                    new Modules.AIEngine.Application.DTOs.ExecutePromptResponseDto(
                        Response: @"{""destinatario"":""Carlos Eduardo"",""blocoUnidade"":""Bloco A - Apto 102"",""codigoRastreio"":""TRK-998877"",""transportadora"":""Amazon Logistics"",""remetente"":""Vendedor Oficial"",""tipo"":""Pacote"",""confiancaPercentual"":95.0}",
                        ModelUsed: "gpt-4o-mini",
                        PromptTokens: 100,
                        CompletionTokens: 50,
                        TotalTokens: 150,
                        DurationMs: 200,
                        Success: true,
                        ErrorMessage: null,
                        ExecutedAt: DateTimeOffset.UtcNow)));

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAiOrchestratorService)))
                .Returns(aiOrchestratorMock.Object);

            var visionOcrService1 = new PackageVisionOcrService(serviceProviderMock.Object, encomendaService1);
            var plugin1 = new PackageVisionPlugin(visionOcrService1);

            var jsonResult = await plugin1.ReadPackageLabelAndNotifyAsync(
                imagemEtiqueta: "https://example.com/etiqueta-amazon.jpg",
                enviarNotificacao: true,
                recebidoPorNome: "Portaria IA Teste",
                condoId: 1);

            jsonResult.Should().NotBeNullOrEmpty();

            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            root.GetProperty("sucesso").GetBoolean().Should().BeTrue();
            root.GetProperty("transportadora").GetString().Should().Be("Amazon Logistics");
            root.GetProperty("notificacaoMoradorEnviada").GetBoolean().Should().BeTrue();

            encomendaIdTenant1 = root.GetProperty("encomendaId").GetInt32();
            encomendaIdTenant1.Should().BeGreaterThan(0);

            // Valida se foi salvo no PostgreSQL real com os campos de Visão/OCR
            var encomendaSalva = await dbTenant1.Encomendas.FirstOrDefaultAsync(e => e.Id == encomendaIdTenant1);
            encomendaSalva.Should().NotBeNull();
            encomendaSalva!.FotoEtiquetaUrl.Should().Be("https://example.com/etiqueta-amazon.jpg");
            encomendaSalva.ConfiancaOcr.Should().BeGreaterThan(80.0);
            encomendaSalva.NotificadoEm.Should().NotBeNull();
        }

        // 3. Tenta consultar a partir do Tenant 2 (Garantia de Isolamento Multi-tenant via Global Query Filter)
        await using (var dbTenant2 = CreateDbContext(tenantId: 2))
        {
            var encomendaTenant2 = await dbTenant2.Encomendas
                .FirstOrDefaultAsync(e => e.Id == encomendaIdTenant1);

            encomendaTenant2.Should().BeNull("O Global Query Filter por tenant_id deve impedir que o Tenant 2 visualize encomendas do Tenant 1");
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
