using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Extensões de configuração para suporte a pgvector no Entity Framework Core.
/// Permite uso de tipos vetoriais para buscas de similaridade (RAG).
/// </summary>
public static class VectorDbContextOptionsExtensions
{
    /// <summary>
    /// Habilita suporte a tipos vetoriais do pgvector no Npgsql.
    /// Deve ser invocado ao configurar DbContextOptions com UseNpgsql.
    /// </summary>
    /// <param name="optionsBuilder">Builder de opções do DbContext</param>
    /// <param name="connectionString">String de conexão PostgreSQL</param>
    /// <returns>Builder configurado com suporte a vector</returns>
    /// <example>
    /// options.UseNpgsqlWithVector(connectionString)
    /// </example>
    public static DbContextOptionsBuilder UseNpgsqlWithVector(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        // Registra tipo Vector no Npgsql
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        return optionsBuilder.UseNpgsql(dataSource, npgsqlOptions => npgsqlOptions.UseVector());
    }

    /// <summary>
    /// Habilita suporte a tipos vetoriais do pgvector no Npgsql (sobrecarga genérica).
    /// Deve ser invocado ao configurar DbContextOptions com UseNpgsql.
    /// </summary>
    /// <typeparam name="TContext">Tipo do DbContext</typeparam>
    /// <param name="optionsBuilder">Builder de opções do DbContext</param>
    /// <param name="connectionString">String de conexão PostgreSQL</param>
    /// <returns>Builder configurado com suporte a vector</returns>
    public static DbContextOptionsBuilder<TContext> UseNpgsqlWithVector<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString)
        where TContext : DbContext
    {
        // Registra tipo Vector no Npgsql
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseVector();
        var dataSource = dataSourceBuilder.Build();

        return optionsBuilder.UseNpgsql(dataSource, npgsqlOptions => npgsqlOptions.UseVector());
    }
}
