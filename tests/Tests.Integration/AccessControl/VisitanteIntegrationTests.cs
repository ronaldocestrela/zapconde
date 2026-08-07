using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Domain.Enums;
using Modules.AccessControl.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Tests.Integration.AccessControl;

public sealed class VisitanteIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_accesscontrol_test")
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
    public async Task FluxoCompleto_CadastrarEntradaESaida_DeveGravarEFiltrarNoPostgres()
    {
        // 1. Setup & Migration
        await using (var db = CreateDbContext(tenantId: 10))
        {
            await db.Database.EnsureCreatedAsync();
        }

        // 2. Arrange Service
        var tenantService = new TestCurrentTenantService { TenantId = 10, CondoId = 1 };
        await using var dbContext = CreateDbContext(tenantId: 10);
        var service = new VisitanteApplicationService(dbContext, tenantService);

        // 3. Act - Create Visitor
        var createRequest = new CreateVisitanteRequestDto(
            NomeCompleto: "Fernanda Costa",
            Documento: "999.888.777-66",
            Telefone: "+5575999887766",
            Tipo: TipoVisitante.PrestadorServico,
            UnidadeId: 204,
            BlocoUnidade: "Bloco C - Apt 204",
            MoradorId: 12,
            DataHoraInicioAutorizacao: DateTimeOffset.UtcNow,
            DataHoraFimAutorizacao: DateTimeOffset.UtcNow.AddHours(4),
            Empresa: "Manutenção Elevadores SA",
            PlacaVeiculo: "XYZ-9876",
            Observacoes: "Manutenção mensal preventiva"
        );

        var createResult = await service.AuthorizeVisitanteAsync(createRequest);
        createResult.IsSuccess.Should().BeTrue();
        var visitanteId = createResult.Data!.Id;
        createResult.Data!.Status.Should().Be(StatusVisitante.Agendado);

        // 4. Act - Register Entry
        var entryResult = await service.RegistrarEntradaAsync(visitanteId, operadorId: 42);
        entryResult.IsSuccess.Should().BeTrue();
        entryResult.Data!.Status.Should().Be(StatusVisitante.Presente);
        entryResult.Data!.DataHoraEntrada.Should().NotBeNull();

        // 5. Act - Register Exit
        var exitResult = await service.RegistrarSaidaAsync(visitanteId, operadorId: 43);
        exitResult.IsSuccess.Should().BeTrue();
        exitResult.Data!.Status.Should().Be(StatusVisitante.Finalizado);
        exitResult.Data!.DataHoraSaida.Should().NotBeNull();

        // 6. Assert - Query List & Summary
        var listResult = await service.GetVisitantesAsync(busca: "Fernanda");
        listResult.IsSuccess.Should().BeTrue();
        listResult.Data.Should().HaveCount(1);

        var summaryResult = await service.GetSummaryAsync();
        summaryResult.IsSuccess.Should().BeTrue();
        summaryResult.Data!.TotalHoje.Should().Be(1);
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
