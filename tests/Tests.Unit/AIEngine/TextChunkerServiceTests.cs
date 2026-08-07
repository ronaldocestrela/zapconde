using FluentAssertions;
using Modules.AIEngine.Infrastructure.Services;
using Xunit;

namespace Tests.Unit.AIEngine;

public class TextChunkerServiceTests
{
    private readonly TextChunkerService _service = new();

    [Fact]
    public void Should_Return_Single_Chunk_When_Text_Is_Short()
    {
        // Arrange
        var text = "É proibido som alto após as 22h nas áreas comuns.";

        // Act
        var result = _service.ChunkText(text, maxChunkSize: 500, overlap: 50);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be(text);
    }

    [Fact]
    public void Should_Split_Long_Text_Into_Multiple_Chunks()
    {
        // Arrange
        var text = string.Join("\n\n", Enumerable.Range(1, 10)
            .Select(i => $"Cláusula {i}: Regra detalhada número {i} sobre a utilização do salão de festas e horário limite de funcionamento até as 22h."));

        // Act
        var result = _service.ChunkText(text, maxChunkSize: 200, overlap: 30);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        result.All(c => c.Length <= 300).Should().BeTrue();
    }

    [Fact]
    public void Should_Return_Empty_List_When_Text_Is_Null_Or_Empty()
    {
        // Act
        var resultEmpty = _service.ChunkText(string.Empty);
        var resultNull = _service.ChunkText(null!);

        // Assert
        resultEmpty.Should().BeEmpty();
        resultNull.Should().BeEmpty();
    }
}
