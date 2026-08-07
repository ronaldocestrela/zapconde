using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Infrastructure.Persistence;
using Modules.AIEngine.Infrastructure.Services;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Tests.Integration.AIEngine;

public sealed class RagPipelineIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg17")
        .WithDatabase("smartcondo_rag_test")
        .WithUsername("smartcondo")
        .WithPassword("smartcondo")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        // Habilita extensão pgvector e schema ai
        await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE EXTENSION IF NOT EXISTS vector; CREATE SCHEMA IF NOT EXISTS ai;";
        await command.ExecuteNonQueryAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    private AiDbContext CreateDbContext(int tenantId)
    {
        var tenantService = new TestCurrentTenantService { TenantId = tenantId };
        var options = new DbContextOptionsBuilder<AiDbContext>()
            .UseNpgsql(
                _postgresContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.UseVector())
            .Options;

        return new AiDbContext(options, tenantService);
    }

    [Fact]
    public async Task RAG_Pipeline_Should_Persist_Document_Chunks_And_Execute_Pgvector_Similarity_Query()
    {
        // Arrange
        var tenantId = 1;
        await using var context = CreateDbContext(tenantId);
        await context.Database.EnsureCreatedAsync();

        var chunker = new TextChunkerService();
        var embeddingService = new TextEmbeddingService();
        var service = new KnowledgeBaseService(context, new TestCurrentTenantService { TenantId = tenantId }, chunker, embeddingService);

        // Act 1 - Cadastra o Regimento Interno
        var uploadRequest = new UploadKnowledgeDocumentRequest(
            Title: "Regimento Interno do Condomínio Parque das Águas",
            DocumentType: KnowledgeDocumentType.RegimentoInterno,
            Content: "É estritamente proibido fazer barulho excessivo ou utilizar som alto nas áreas comuns após as 22h. A piscina funciona diariamente das 06h às 20h.");

        var uploadResult = await service.UploadAndProcessDocumentAsync(uploadRequest);

        // Assert 1
        uploadResult.IsSuccess.Should().BeTrue();
        uploadResult.Data.Should().NotBeNull();
        uploadResult.Data!.ChunkCount.Should().BeGreaterThan(0);

        // Act 2 - Executa busca semântica RAG por similaridade vetorial via pgvector
        var searchRequest = new KnowledgeSearchQueryRequest(
            QueryText: "Qual o horário de funcionamento da piscina?",
            TopK: 2,
            MinSimilarity: 0.0);

        var searchResult = await service.SearchSimilarChunksAsync(searchRequest);

        // Assert 2
        searchResult.IsSuccess.Should().BeTrue();
        searchResult.Data.Should().NotBeNull();
        searchResult.Data.Should().NotBeEmpty();
        searchResult.Data![0].DocumentTitle.Should().Contain("Regimento Interno");
        searchResult.Data![0].SimilarityScore.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public async Task RAG_Pipeline_Should_Enforce_MultiTenant_Isolation()
    {
        // Arrange
        await using (var contextInit = CreateDbContext(1))
        {
            await contextInit.Database.EnsureCreatedAsync();
        }

        var chunker = new TextChunkerService();
        var embeddingService = new TextEmbeddingService();

        // Condomínio 1
        await using (var contextTenant1 = CreateDbContext(1))
        {
            var serviceTenant1 = new KnowledgeBaseService(contextTenant1, new TestCurrentTenantService { TenantId = 1 }, chunker, embeddingService);
            await serviceTenant1.UploadAndProcessDocumentAsync(new UploadKnowledgeDocumentRequest(
                Title: "Regimento Condomínio 1",
                DocumentType: KnowledgeDocumentType.RegimentoInterno,
                Content: "Documento exclusivo do Condomínio 1"));
        }

        // Condomínio 2
        await using (var contextTenant2 = CreateDbContext(2))
        {
            var serviceTenant2 = new KnowledgeBaseService(contextTenant2, new TestCurrentTenantService { TenantId = 2 }, chunker, embeddingService);
            await serviceTenant2.UploadAndProcessDocumentAsync(new UploadKnowledgeDocumentRequest(
                Title: "Regimento Condomínio 2",
                DocumentType: KnowledgeDocumentType.RegimentoInterno,
                Content: "Documento exclusivo do Condomínio 2"));
        }

        // Act & Assert - Condomínio 1 vê apenas seus documentos
        await using (var contextReadTenant1 = CreateDbContext(1))
        {
            var serviceReadTenant1 = new KnowledgeBaseService(contextReadTenant1, new TestCurrentTenantService { TenantId = 1 }, chunker, embeddingService);
            var docsTenant1 = await serviceReadTenant1.GetDocumentsAsync();

            docsTenant1.IsSuccess.Should().BeTrue();
            docsTenant1.Data.Should().HaveCount(1);
            docsTenant1.Data![0].Title.Should().Be("Regimento Condomínio 1");
        }

        // Act & Assert - Condomínio 2 vê apenas seus documentos
        await using (var contextReadTenant2 = CreateDbContext(2))
        {
            var serviceReadTenant2 = new KnowledgeBaseService(contextReadTenant2, new TestCurrentTenantService { TenantId = 2 }, chunker, embeddingService);
            var docsTenant2 = await serviceReadTenant2.GetDocumentsAsync();

            docsTenant2.IsSuccess.Should().BeTrue();
            docsTenant2.Data.Should().HaveCount(1);
            docsTenant2.Data![0].Title.Should().Be("Regimento Condomínio 2");
        }
    }

    private class TestCurrentTenantService : ICurrentTenantService
    {
        public int? TenantId { get; set; } = 1;
        public int? CondoId { get; set; } = 1;
        public void SetTenantId(int tenantId) => TenantId = tenantId;
        public void SetCondoId(int condoId) => CondoId = condoId;
        public void Clear() { TenantId = null; CondoId = null; }
    }
}
