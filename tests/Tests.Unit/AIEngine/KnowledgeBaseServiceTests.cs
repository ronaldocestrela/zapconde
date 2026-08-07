using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Modules.AIEngine.Application.DTOs;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Domain.Exceptions;
using Modules.AIEngine.Infrastructure.Persistence;
using Modules.AIEngine.Infrastructure.Services;
using Pgvector;
using Xunit;

namespace Tests.Unit.AIEngine;

public class KnowledgeBaseServiceTests
{
    private readonly Mock<ICurrentTenantService> _tenantServiceMock = new();
    private readonly Mock<ITextChunkerService> _chunkerMock = new();
    private readonly Mock<ITextEmbeddingService> _embeddingMock = new();

    public KnowledgeBaseServiceTests()
    {
        _tenantServiceMock.Setup(t => t.TenantId).Returns(1);
        _chunkerMock.Setup(c => c.ChunkText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns((string text, int max, int ov) => new List<string> { text });
        _embeddingMock.Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), default))
            .ReturnsAsync(TextEmbeddingService.GenerateDeterministicVector("test"));
    }

    private AiDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AiDbContext(options, _tenantServiceMock.Object);
    }

    [Fact]
    public async Task Should_Upload_And_Process_KnowledgeDocument_Successfully()
    {
        // Arrange
        await using var dbContext = CreateInMemoryDbContext();
        var service = new KnowledgeBaseService(dbContext, _tenantServiceMock.Object, _chunkerMock.Object, _embeddingMock.Object);

        var request = new UploadKnowledgeDocumentRequest(
            Title: "Regimento Interno 2026",
            DocumentType: KnowledgeDocumentType.RegimentoInterno,
            Content: "É proibido barulho excessivo após as 22h nas áreas comuns.");

        // Act
        var result = await service.UploadAndProcessDocumentAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Regimento Interno 2026");
        result.Data.ChunkCount.Should().Be(1);

        var docInDb = await dbContext.KnowledgeDocuments.Include(d => d.Chunks).FirstOrDefaultAsync();
        docInDb.Should().NotBeNull();
        docInDb!.Chunks.Should().HaveCount(1);
    }

    [Fact]
    public async Task Should_Fail_Upload_When_Title_Is_Empty()
    {
        // Arrange
        await using var dbContext = CreateInMemoryDbContext();
        var service = new KnowledgeBaseService(dbContext, _tenantServiceMock.Object, _chunkerMock.Object, _embeddingMock.Object);

        var request = new UploadKnowledgeDocumentRequest(
            Title: "",
            DocumentType: KnowledgeDocumentType.RegimentoInterno,
            Content: "Conteúdo");

        // Act
        var result = await service.UploadAndProcessDocumentAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("O título do documento é obrigatório.");
    }

    [Fact]
    public void KnowledgeDocument_Should_Throw_Exception_When_TenantId_Invalid()
    {
        // Act & Assert
        Action act = () => KnowledgeDocument.Criar(0, "Titulo", KnowledgeDocumentType.RegimentoInterno, "Conteudo");
        act.Should().Throw<AiEngineDomainException>().WithMessage("*TenantId*");
    }

    [Fact]
    public void KnowledgeChunk_Should_Throw_Exception_When_Content_Empty()
    {
        // Act & Assert
        Action act = () => KnowledgeChunk.Criar(1, 1, 0, "");
        act.Should().Throw<AiEngineDomainException>().WithMessage("*conteúdo*");
    }
}
