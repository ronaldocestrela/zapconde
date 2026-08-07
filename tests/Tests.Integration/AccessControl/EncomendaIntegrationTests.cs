using System;
using System.Linq;
using System.Threading.Tasks;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.AccessControl;

public sealed class EncomendaIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_encomenda_test")
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
    public async Task FluxoCompleto_RegistrarRecebimentoNotificarEBaixa_DeveGravarEFiltrarNoPostgres()
    {
        // 1. Setup & Schema
        await using (var db = CreateDbContext(tenantId: 5))
        {
            await db.Database.EnsureCreatedAsync();
        }

        // 2. Arrange Service
        var tenantService = new TestCurrentTenantService { TenantId = 5, CondoId = 2 };
        await using var dbContext = CreateDbContext(tenantId: 5, condoId: 2);
        var service = new EncomendaApplicationService(dbContext, tenantService);

        // 3. Act - Registrar Recebimento
        var reqRecebimento = new RegistrarRecebimentoEncomendaRequest(
            CondoId: 2,
            UnidadeId: 202,
            BlocoUnidade: "Bloco B - Apt 202",
            CodigoRastreio: "LOG123456789",
            Descricao: "Teclado Mecânico",
            Remetente: "Kabum",
            Transportadora: "Loggi",
            LocalArmazenamento: "Prateleira A1",
            Tipo: TipoEncomenda.Caixa,
            RecebidoPorNome: "Marcos Porteiro",
            DataRecebimento: DateTimeOffset.UtcNow.AddMinutes(-30),
            Observacoes: "Caixa em perfeito estado");

        var resultRecebimento = await service.RegistrarRecebimentoAsync(reqRecebimento);

        // Assert Recebimento
        resultRecebimento.IsSuccess.Should().BeTrue();
        resultRecebimento.Data.Should().NotBeNull();
        var encomendaId = resultRecebimento.Data!.Id;
        resultRecebimento.Data.Status.Should().Be(StatusEncomenda.AguardandoRetirada);
        resultRecebimento.Data.BlocoUnidade.Should().Be("Bloco B - Apt 202");

        // 4. Act - Notificar Morador
        var resultNotificacao = await service.NotificarMoradorAsync(encomendaId);

        // Assert Notificação
        resultNotificacao.IsSuccess.Should().BeTrue();
        resultNotificacao.Data!.NotificadoEm.Should().NotBeNull();

        // 5. Act - Registrar Baixa (Entrega ao Morador)
        var reqBaixa = new RegistrarBaixaEncomendaRequest(
            RetiradoPorNome: "Fernando Silva",
            DataRetirada: DateTimeOffset.UtcNow);

        var resultBaixa = await service.RegistrarBaixaAsync(encomendaId, reqBaixa);

        // Assert Baixa
        resultBaixa.IsSuccess.Should().BeTrue();
        resultBaixa.Data!.Status.Should().Be(StatusEncomenda.Entregue);
        resultBaixa.Data.RetiradoPorNome.Should().Be("Fernando Silva");
        resultBaixa.Data.DataRetirada.Should().NotBeNull();

        // 6. Act - Obter KPI Summary
        var summaryResult = await service.GetSummaryAsync();
        summaryResult.IsSuccess.Should().BeTrue();
        summaryResult.Data!.TotalEncomendas.Should().Be(1);
        summaryResult.Data.EntreguesHoje.Should().Be(1);
        summaryResult.Data.AguardandoRetirada.Should().Be(0);
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
