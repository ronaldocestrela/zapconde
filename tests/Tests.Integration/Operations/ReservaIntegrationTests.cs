using BuildingBlocks.Infrastructure.Caching;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;
using Modules.Operations.Infrastructure.Persistence;
using Modules.Operations.Infrastructure.Persistence.Repositories;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Tests.Integration.Operations;
using Xunit;

namespace Tests.Integration.Operations;

public sealed class ReservaIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_reservas_test")
        .WithUsername("smartcondo")
        .WithPassword("smartcondo")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7.4-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _redisContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgresContainer.DisposeAsync().AsTask(),
            _redisContainer.DisposeAsync().AsTask()
        );
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

    private RedisDistributedLockService CreateLockService()
    {
        var multiplexer = ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString());
        return new RedisDistributedLockService(multiplexer);
    }

    [Fact]
    public async Task ReservaService_Should_Create_Reserva_And_Prevent_Collision_With_RedisLock()
    {
        // Arrange - Criar Schema e Área Comum
        int areaComumId;
        await using (var setupCtx = CreateDbContext(1))
        {
            await setupCtx.Database.EnsureCreatedAsync();

            var areaRepo = new AreaComumRepository(setupCtx);
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var areaAppService = new AreaComumApplicationService(areaRepo, tenantService);

            var createAreaReq = new CreateAreaComumRequest(
                CondoId: 1,
                Nome: "Salão de Festas Principal",
                Descricao: "Salão nobre",
                Tipo: TipoAreaComum.Eventos,
                CapacidadeMaxima: 100,
                TaxaReserva: 150.00m,
                TaxaLimpeza: 50.00m,
                HorarioInicioFuncionamento: "08:00",
                HorarioFimFuncionamento: "22:00",
                TempoAntecedenciaMinimaDias: 1,
                TempoAntecedenciaMaximaDias: 60,
                RequerAprovacaoSindico: false,
                RegrasUso: "Sem barulho excessivo.");

            var areaRes = await areaAppService.CreateAsync(createAreaReq);
            areaRes.IsSuccess.Should().BeTrue();
            areaComumId = areaRes.Data.Id;
        }

        var dataRef = DateTime.UtcNow.Date.AddDays(5);
        var dataInicio = dataRef.AddHours(14);
        var dataFim = dataRef.AddHours(18);

        // Act 1: Criar a primeira reserva no horário (14:00 às 18:00)
        await using (var ctx1 = CreateDbContext(1))
        {
            var areaRepo = new AreaComumRepository(ctx1);
            var reservaRepo = new ReservaRepository(ctx1);
            var lockService = CreateLockService();
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var appService = new ReservaApplicationService(reservaRepo, areaRepo, lockService, tenantService);

            var req1 = new CreateReservaRequest(
                CondoId: 1,
                AreaComumId: areaComumId,
                MoradorId: 10,
                NomeMorador: "João Silva",
                UnidadeMorador: "Apt 101",
                DataInicio: dataInicio,
                DataFim: dataFim,
                QuantidadePessoas: 40,
                Observacao: "Festa de aniversário");

            var res1 = await appService.CriarReservaAsync(req1);
            res1.IsSuccess.Should().BeTrue();
            res1.Data.Status.Should().Be(StatusReserva.Confirmada);
            res1.Data.ValorTotal.Should().Be(200.00m);
        }

        // Act 2: Tentar criar uma segunda reserva com sobreposição temporal no mesmo espaço (16:00 às 20:00)
        await using (var ctx2 = CreateDbContext(1))
        {
            var areaRepo = new AreaComumRepository(ctx2);
            var reservaRepo = new ReservaRepository(ctx2);
            var lockService = CreateLockService();
            var tenantService = new TestCurrentTenantService { TenantId = 1, CondoId = 1 };
            var appService = new ReservaApplicationService(reservaRepo, areaRepo, lockService, tenantService);

            var req2 = new CreateReservaRequest(
                CondoId: 1,
                AreaComumId: areaComumId,
                MoradorId: 20,
                NomeMorador: "Maria Santos",
                UnidadeMorador: "Apt 302",
                DataInicio: dataInicio.AddHours(2), // 16:00 (sobrepõe com 14:00-18:00)
                DataFim: dataFim.AddHours(2),    // 20:00
                QuantidadePessoas: 25,
                Observacao: "Reunião de amigos");

            var res2 = await appService.CriarReservaAsync(req2);
            res2.IsSuccess.Should().BeFalse();
            res2.Message.Should().Contain("Já existe uma reserva");
        }

        // Act 3: Verificar isolamento multi-tenant: Tenant 2 não enxerga as reservas do Tenant 1
        await using (var ctxTenant2 = CreateDbContext(2))
        {
            var reservaRepo = new ReservaRepository(ctxTenant2);
            var areaRepo = new AreaComumRepository(ctxTenant2);
            var lockService = CreateLockService();
            var tenantService = new TestCurrentTenantService { TenantId = 2, CondoId = 1 };
            var appService = new ReservaApplicationService(reservaRepo, areaRepo, lockService, tenantService);

            var listRes = await appService.ListarReservasAsync(condoId: 1);
            listRes.IsSuccess.Should().BeTrue();
            listRes.Data.Should().BeEmpty("Reservas do Tenant 1 não devem ser visíveis para o Tenant 2.");
        }
    }
}
