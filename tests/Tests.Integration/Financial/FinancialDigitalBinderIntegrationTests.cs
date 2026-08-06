using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Financial.Application.DTOs;
using Modules.Financial.Application.Services;
using Modules.Financial.Domain.Entities;
using Modules.Financial.Domain.Enums;
using Modules.Financial.Domain.Services;
using Modules.Financial.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Financial;

public sealed class FinancialDigitalBinderIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_digitalbinder_test")
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
    public async Task PastaDigitalApplicationService_DeveCriarAnexarESubmeterComIsolamentoMultiTenant()
    {
        // 1. Setup schema
        await using (var seedContext = CreateDbContext(1))
        {
            await seedContext.Database.EnsureCreatedAsync();
        }

        // 2. Criar Pasta Digital no Tenant 1
        int pastaId;
        await using (var contextTenant1 = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var appService = new PastaDigitalApplicationService(contextTenant1, tenantService);

            var createResult = await appService.CriarPastaDigitalAsync(new CriarPastaDigitalRequestDto(10, 2026, 7, 5000m, "Resumo Teste"));
            createResult.IsSuccess.Should().BeTrue();
            pastaId = createResult.Data!.Id;

            // Adicionar Item de Balancete
            var addResult = await appService.AdicionarItemBalanceteAsync(pastaId, new AdicionarItemBalanceteRequestDto(
                TipoLancamentoBalancete.Receita, CategoriaPlanoContas.ReceitaOrdinaria, "Taxa Condominial Julho", 10000m, 10500m, DateTime.UtcNow));
            addResult.IsSuccess.Should().BeTrue();
            addResult.Data!.SaldoMes.Should().Be(10500m);

            // Anexar Documento
            var docResult = await appService.AnexarDocumentoAsync(pastaId, new AnexarDocumentoRequestDto(
                CategoriaDocumentoPrestacao.ExtratoBancario, "Extrato Julho", "extrato_jul2026.pdf", "http://storage/extrato.pdf", "application/pdf", 1024, 1));
            docResult.IsSuccess.Should().BeTrue();

            // Submeter
            var submitResult = await appService.SubmeterParaConselhoAsync(pastaId);
            submitResult.IsSuccess.Should().BeTrue();
            submitResult.Data!.Status.Should().Be(StatusPastaDigital.EmAnaliseConselho);
        }

        // 3. Garantir que Tenant 2 NÃO consegue acessar a pasta do Tenant 1
        await using (var contextTenant2 = CreateDbContext(2))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 2, CondoId = 10 };
            var appService = new PastaDigitalApplicationService(contextTenant2, tenantService);

            var getResult = await appService.ObterPorIdAsync(pastaId);
            getResult.IsSuccess.Should().BeFalse();

            var listResult = await appService.ListarPorCondominioAsync(10);
            listResult.IsSuccess.Should().BeTrue();
            listResult.Data.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task ConciliacaoBancariaApplicationService_DeveImportarEConciliarExtrato()
    {
        // 1. Setup schema
        await using (var seedContext = CreateDbContext(1))
        {
            await seedContext.Database.EnsureCreatedAsync();

            var fatura = Fatura.Create(1, 10, 101, 5, "2026-07", new DateTime(2026, 7, 10));
            fatura.AddItem("Taxa Condominial Julho", TipoItemCobranca.TaxaCondominial, 500m);
            fatura.RegistrarPagamento(new DateTime(2026, 7, 10), 500m);
            seedContext.Faturas.Add(fatura);
            await seedContext.SaveChangesAsync();
        }

        int contaId;
        // 2. Importar e Conciliar
        await using (var context = CreateDbContext(1))
        {
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
            var domainService = new ConciliacaoBancariaDomainService();
            var appService = new ConciliacaoBancariaApplicationService(context, tenantService, domainService);

            var contaResult = await appService.CriarContaBancariaAsync(new CriarContaBancariaRequestDto(10, "Banco do Brasil", "001", "1234", "56789-0"));
            contaResult.IsSuccess.Should().BeTrue();
            contaId = contaResult.Data!.Id;

            var importResult = await appService.ImportarExtratoAsync(new ImportarExtratoRequestDto(contaId, new List<ImportarExtratoItemDto>
            {
                new ImportarExtratoItemDto(new DateTime(2026, 7, 10), "DEP PIX COND 101", "PIX123", 500m, TipoTransacaoBancaria.Credito)
            }));
            importResult.IsSuccess.Should().BeTrue();

            var autoResult = await appService.ProcessarConciliacaoAutomaticaAsync(contaId);
            autoResult.IsSuccess.Should().BeTrue();
            autoResult.Data!.ConciliadosAutomaticamente.Should().Be(1);
        }
    }

    private sealed class TestCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId { get; set; }
        public int? CondoId { get; set; }

        public void SetTenantId(int tenantId) => TenantId = tenantId;
        public void SetCondoId(int condoId) => CondoId = condoId;
        public void Clear()
        {
            TenantId = null;
            CondoId = null;
        }
    }
}
