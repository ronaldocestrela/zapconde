using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Infrastructure.Persistence;
using Modules.Operations.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.Operations;

public sealed class OcorrenciaIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_ocorrencia_test")
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
    public async Task OcorrenciaService_Should_Create_AddAttachments_UpdateStatus_And_Enforce_Tenant_Isolation()
    {
        // 1. Setup Database Schema
        using (var setupContext = CreateDbContext(tenantId: 1, condoId: 1))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        // 2. Tenant 1 Creates a Ticket with attachments
        var tenant1Service = new TestCurrentTenantService { TenantId = 1, CondoId = 100 };
        using (var tenant1Context = CreateDbContext(tenant1Service.TenantId, tenant1Service.CondoId))
        {
            var repository = new OcorrenciaRepository(tenant1Context);
            var appService = new OcorrenciaApplicationService(repository, tenant1Service);

            var createRequest = new CriarOcorrenciaRequest(
                CondoId: 100,
                MoradorId: "morador-101",
                MoradorNome: "Ana Paula",
                Titulo: "Vazamento no teto da cozinha",
                Descricao: "Infiltração proveniente do apartamento superior",
                Categoria: CategoriaOcorrencia.Manutencao,
                Prioridade: PrioridadeOcorrencia.Alta,
                Localizacao: "Bloco A - Apto 101",
                AnexosIniciais: new List<CriarAnexoDto>
                {
                    new("/uploads/foto_vazamento.jpg", "foto_vazamento.jpg", "image/jpeg", 512000)
                }
            );

            var createResult = await appService.CriarOcorrenciaAsync(createRequest);
            createResult.IsSuccess.Should().BeTrue(createResult.Message);
            createResult.Data.Should().NotBeNull();
            createResult.Data!.Status.Should().Be(StatusOcorrencia.Aberta);
            createResult.Data.Anexos.Should().HaveCount(1);
            createResult.Data.Historico.Should().HaveCount(1);

            var ticketId = createResult.Data.Id;

            // Update status (Aberta -> EmAndamento)
            var updateResult = await appService.AtualizarStatusAsync(ticketId, new AtualizarStatusOcorrenciaRequest(
                NovoStatus: StatusOcorrencia.EmAndamento,
                Comentario: "Zelador iniciou a vistoria",
                UsuarioId: "zelador-01",
                UsuarioNome: "Zelador Marcos"
            ));

            updateResult.IsSuccess.Should().BeTrue(updateResult.Message);
            updateResult.Data!.Status.Should().Be(StatusOcorrencia.EmAndamento);
            updateResult.Data.Historico.Should().HaveCount(2);

            // Fetch metrics
            var metricsResult = await appService.ObterResumoMetricasAsync(100);
            metricsResult.IsSuccess.Should().BeTrue(metricsResult.Message);
            metricsResult.Data!.Total.Should().Be(1);
            metricsResult.Data.EmAndamento.Should().Be(1);
        }

        // 3. Tenant 2 tries to access Tenant 1 Ticket -> Query Filter enforces isolation
        var tenant2Service = new TestCurrentTenantService { TenantId = 2, CondoId = 100 };
        using (var tenant2Context = CreateDbContext(tenant2Service.TenantId, tenant2Service.CondoId))
        {
            var repository = new OcorrenciaRepository(tenant2Context);
            var appService = new OcorrenciaApplicationService(repository, tenant2Service);

            var listResult = await appService.ListarAsync(100);
            listResult.IsSuccess.Should().BeTrue(listResult.Message);
            listResult.Data.Should().BeEmpty("Filtro global de tenant deve impedir vazamento de chamados entre condomínios de tenants distintos.");
        }
    }
}
