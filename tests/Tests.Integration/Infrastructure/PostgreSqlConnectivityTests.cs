using Npgsql;
using Testcontainers.PostgreSql;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Testes de integração para conectividade básica com PostgreSQL
/// conforme Subfase 1.2.1 do ROADMAP.
/// </summary>
public sealed class PostgreSqlConnectivityTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("smartcondo_test")
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
    public async Task PostgreSqlContainer_Should_Accept_NpgsqlConnection()
    {
        // Arrange
        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());

        // Act
        await connection.OpenAsync();

        // Assert
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
    }
}
