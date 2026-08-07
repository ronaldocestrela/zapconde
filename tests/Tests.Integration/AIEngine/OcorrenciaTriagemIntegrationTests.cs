using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.AIEngine.Application.Plugins;
using Modules.AIEngine.Application.Services;
using Modules.Operations.Application.Services;
using Modules.Operations.Infrastructure.Persistence;
using Modules.Operations.Infrastructure.Persistence.Repositories;
using Moq;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.AIEngine;

public sealed class OcorrenciaTriagemIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_ocorrencia_triagem_test")
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
    public async Task OcorrenciaTriagemPlugin_DeveTriarEAbrirOcorrenciaNoBancoRealEIsolarPorTenant()
    {
        // 1. Setup DB Schema
        await using (var db = CreateDbContext(tenantId: 1))
        {
            await db.Database.EnsureCreatedAsync();
        }

        Guid ocorrenciaTenant1Id;

        // 2. Executa a triagem e abertura no Tenant 1
        await using (var dbTenant1 = CreateDbContext(tenantId: 1))
        {
            var tenantServiceMock = new Mock<ICurrentTenantService>();
            tenantServiceMock.Setup(t => t.TenantId).Returns(1);
            tenantServiceMock.Setup(t => t.CondoId).Returns(1);

            var repository = new OcorrenciaRepository(dbTenant1);
            var appService = new OcorrenciaApplicationService(repository, tenantServiceMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var triagemService = new OcorrenciaTriagemService(serviceProviderMock.Object, appService);
            var plugin = new OcorrenciaTriagemPlugin(triagemService);

            var jsonResult = await plugin.TriarEAbrirOcorrenciaAsync(
                fotoUrl: "https://storage.smartcondo.com/evidences/infiltracao-garagem-b2.jpg",
                relatoTexto: "Infiltração com pingos constantes caindo sobre a vaga 42 no subsolo 2",
                moradorId: "morador-101",
                moradorNome: "Carlos Mendes",
                condoId: 1
            );

            jsonResult.Should().Contain("sucesso\":true");
            jsonResult.Should().Contain("Infiltração");
            jsonResult.Should().Contain("Manutencao");

            var savedOcorrencia = await dbTenant1.Ocorrencias.Include(o => o.Anexos).FirstOrDefaultAsync(o => o.MoradorId == "morador-101");
            savedOcorrencia.Should().NotBeNull();
            savedOcorrencia!.TenantId.Should().Be(1);
            savedOcorrencia.Titulo.Should().Contain("Infiltração");
            savedOcorrencia.OrigemTriagemIa.Should().NotBeNullOrEmpty();
            savedOcorrencia.ConfiancaTriagemIa.Should().BeGreaterThan(0.8);
            savedOcorrencia.Anexos.Should().NotBeEmpty();

            ocorrenciaTenant1Id = savedOcorrencia.Id;
        }

        // 3. Valida isolamento de Multi-Tenancy (Tenant 2 não enxerga a ocorrência do Tenant 1)
        await using (var dbTenant2 = CreateDbContext(tenantId: 2))
        {
            var ocorrenciaTenant2View = await dbTenant2.Ocorrencias.FirstOrDefaultAsync(o => o.Id == ocorrenciaTenant1Id);
            ocorrenciaTenant2View.Should().BeNull("O filtro global de multi-tenancy deve impedir o Tenant 2 de ler dados do Tenant 1.");
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
