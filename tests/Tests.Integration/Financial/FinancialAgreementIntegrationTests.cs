using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.Dtos;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Financial;

public sealed class FinancialAgreementIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_agreement_test")
        .WithUsername("smartcondo")
        .WithPassword("smartcondo")
        .Build();

    public async Task InitializeAsync() => await _postgresContainer.StartAsync();
    public async Task DisposeAsync() => await _postgresContainer.DisposeAsync();

    private FinancialDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService { TenantId = tenantId, CondoId = condoId };
        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new FinancialDbContext(options, tenantService);
    }

    [Fact]
    public async Task AcordoApplicationService_DeveCriarEfetivarEIsolarAcordosPorTenant()
    {
        // Setup schema
        await using (var seed = CreateDbContext(1))
        {
            await seed.Database.EnsureCreatedAsync();

            // Seed faturas no Tenant 1
            var fatura1 = Fatura.Create(1, 1, 101, 5, "2026-06", DateTime.UtcNow.AddDays(-60));
            fatura1.AddItem("Taxa Junho", TipoItemCobranca.TaxaCondominial, 300m);
            fatura1.Status = StatusFatura.Vencido;

            var fatura2 = Fatura.Create(1, 1, 101, 5, "2026-07", DateTime.UtcNow.AddDays(-30));
            fatura2.AddItem("Taxa Julho", TipoItemCobranca.TaxaCondominial, 300m);
            fatura2.Status = StatusFatura.Vencido;

            seed.Faturas.AddRange(fatura1, fatura2);
            await seed.SaveChangesAsync();
        }

        int acordoId;
        // 1. Efetivar Acordo no Tenant 1
        await using (var contextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var calc = new CalculadoraAcordoDomainService();
            var acordoService = new AcordoApplicationService(contextTenant1, tenantService, calc);

            var req = new CriarAcordoRequest(
                CondoId: 1,
                UnidadeId: 101,
                MoradorId: 5,
                FaturasIds: new List<int> { 1, 2 },
                ValorDescontoConcedido: 100m,
                QuantidadeParcelas: 2,
                DataPrimeiroVencimento: DateTime.UtcNow.AddDays(5),
                Observacoes: "Acordo de teste de integração"
            );

            var res = await acordoService.CriarAcordoAsync(req);
            res.IsSuccess.Should().BeTrue();
            res.Data.Should().NotBeNull();
            res.Data!.Status.Should().Be(StatusAcordo.Ativo);
            res.Data.ValorTotalOriginal.Should().Be(600m);
            res.Data.ValorTotalAcordo.Should().Be(500m);
            res.Data.Parcelas.Should().HaveCount(2);

            acordoId = res.Data.Id;
        }

        // 2. Verificar que faturas originais ficaram EmAcordo
        await using (var verifyContext = CreateDbContext(1))
        {
            var faturas = await verifyContext.Faturas.Where(f => f.UnidadeId == 101).ToListAsync();
            faturas.Should().OnlyContain(f => f.Status == StatusFatura.EmAcordo);
        }

        // 3. Garantir que Tenant 2 NÃO enxerga o acordo
        await using (var contextTenant2 = CreateDbContext(2))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 2, CondoId = 1 };
            var calc = new CalculadoraAcordoDomainService();
            var acordoService = new AcordoApplicationService(contextTenant2, tenantService, calc);

            var listRes = await acordoService.ObterAcordosPorCondominioAsync(1);
            listRes.IsSuccess.Should().BeTrue();
            listRes.Data.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task ReguaInadimplenciaAppService_DeveExecutarAcoesEDashboard()
    {
        await using (var seed = CreateDbContext(1))
        {
            await seed.Database.EnsureCreatedAsync();

            var fatura = Fatura.Create(1, 1, 102, 6, "2026-07", DateTime.UtcNow.AddDays(-15));
            fatura.AddItem("Taxa Inadimplente", TipoItemCobranca.TaxaCondominial, 400m);
            fatura.Status = StatusFatura.Vencido;

            seed.Faturas.Add(fatura);
            await seed.SaveChangesAsync();
        }

        // Processar Régua
        await using (var context = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var engine = new ReguaInadimplenciaEngine();
            var reguaService = new ReguaInadimplenciaAppService(context, tenantService, engine);

            var processRes = await reguaService.ProcessarReguaCobrancaAsync(1);
            processRes.IsSuccess.Should().BeTrue();
            processRes.Data!.TotalAcoesProcessadas.Should().BeGreaterThan(0);

            var dashRes = await reguaService.ObterDashboardInadimplenciaAsync(1);
            dashRes.IsSuccess.Should().BeTrue();
            dashRes.Data!.ValorTotalInadimplente.Should().Be(400m);
            dashRes.Data.AgingList.TotalVencido1A30Dias.Should().Be(400m);
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
