using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Operations;

public sealed class AssembleiaIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_assembly_test")
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

    private OperationsDbContext CreateDbContext(int? tenantId, int? condoId = 1)
    {
        var tenantService = new TestCurrentTenantService
        {
            TenantId = tenantId,
            CondoId = condoId
        };

        var options = new DbContextOptionsBuilder<OperationsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        return new OperationsDbContext(options, tenantService);
    }

    [Fact]
    public async Task AssembleiaService_Should_Create_Vote_Enforce_Single_Vote_Generate_Ata_And_MultiTenancy()
    {
        // 1. Setup Database Schema
        using (var setupContext = CreateDbContext(tenantId: 1, condoId: 10))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        // 2. Tenant 1 Creates an Assembly with Pautas
        var tenant1Service = new TestCurrentTenantService { TenantId = 1, CondoId = 10 };
        using (var tenant1Context = CreateDbContext(tenant1Service.TenantId, tenant1Service.CondoId))
        {
            var service = new AssembleiaApplicationService(tenant1Context, tenant1Service);
            var createRequest = new CreateAssembleiaRequest(
                CondoId: 10,
                Titulo: "Assembleia Geral Ordinária 2026",
                Tipo: TipoAssembleia.Ordinaria,
                DataInicio: DateTime.UtcNow,
                DataFim: DateTime.UtcNow.AddDays(2),
                CriadoPorUserId: "user-admin",
                Descricao: "Deliberações anuais de orçamento e contas",
                PautasInicial: new List<CreatePautaInput>
                {
                    new("Aprovação das Contas de 2025", TipoVotacao.MaioriaSimples, "Votação do balanço anual financeiro."),
                    new("Eleição do Síndico", TipoVotacao.MaioriaSimples)
                }
            );

            var createResult = await service.CriarAssembleiaAsync(createRequest);
            createResult.IsSuccess.Should().BeTrue(createResult.Message);
            createResult.Data.Should().NotBeNull();
            createResult.Data.Pautas.Should().HaveCount(2);
            createResult.Data.Status.Should().Be(StatusAssembleia.Agendada);

            var assembleiaId = createResult.Data.Id;
            var pautaContas = createResult.Data.Pautas.First(p => p.Ordem == 1);

            // 3. Start Assembly
            var startResult = await service.AtualizarStatusAsync(assembleiaId, StatusAssembleia.EmAndamento);
            startResult.IsSuccess.Should().BeTrue();
            startResult.Data.Status.Should().Be(StatusAssembleia.EmAndamento);

            // 4. Register Vote for Unit 101
            var vote1Request = new RegistrarVotoRequest("morador-101", "101", "Sim");
            var vote1Result = await service.RegistrarVotoAsync(assembleiaId, pautaContas.Id, vote1Request);
            vote1Result.IsSuccess.Should().BeTrue(vote1Result.Message);

            // 5. Attempt Duplicate Vote for Unit 101 -> Should Fail
            var vote2Request = new RegistrarVotoRequest("outro-morador-101", "101", "Não");
            var vote2Result = await service.RegistrarVotoAsync(assembleiaId, pautaContas.Id, vote2Request);
            vote2Result.IsSuccess.Should().BeFalse();
            vote2Result.Message.Should().Contain("101");

            // 6. Register Vote for Unit 102
            var vote3Request = new RegistrarVotoRequest("morador-102", "102", "Não");
            await service.RegistrarVotoAsync(assembleiaId, pautaContas.Id, vote3Request);

            // 7. Finalize Assembly & Generate Ata
            var finalizeResult = await service.EncerrarEGerarAtaAsync(assembleiaId);
            finalizeResult.IsSuccess.Should().BeTrue();
            finalizeResult.Data.Status.Should().Be(StatusAssembleia.Encerrada);
            finalizeResult.Data.AtaTexto.Should().NotBeNullOrWhiteSpace();
            finalizeResult.Data.AtaTexto.Should().Contain("Quórum Total de Unidades Participantes: 2 unidade(s)");

            // 8. Verify Summary KPI
            var summaryResult = await service.ObterResumoKpiAsync(10);
            summaryResult.IsSuccess.Should().BeTrue();
            summaryResult.Data.Total.Should().Be(1);
            summaryResult.Data.Encerradas.Should().Be(1);
            summaryResult.Data.TotalVotosRegistrados.Should().Be(2);
        }

        // 9. Tenant 2 Isolation Check
        var tenant2Service = new TestCurrentTenantService { TenantId = 2, CondoId = 10 };
        using (var tenant2Context = CreateDbContext(tenant2Service.TenantId, tenant2Service.CondoId))
        {
            var serviceTenant2 = new AssembleiaApplicationService(tenant2Context, tenant2Service);
            var listTenant2 = await serviceTenant2.ListarAsync(10);
            listTenant2.IsSuccess.Should().BeTrue();
            listTenant2.Data.Should().BeEmpty();
        }
    }
}
