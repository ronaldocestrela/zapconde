using BuildingBlocks.Infrastructure.Environment;
using FluentAssertions;
using Xunit;

namespace Tests.Unit.Infrastructure;

public sealed class EnvLoaderTests : IDisposable
{
    private readonly string _tempEnvPath;

    public EnvLoaderTests()
    {
        _tempEnvPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.env");
    }

    public void Dispose()
    {
        if (File.Exists(_tempEnvPath))
        {
            File.Delete(_tempEnvPath);
        }
    }

    [Fact]
    public void LoadFromFile_ShouldParseKeyValuePairsAndIgnoreComments()
    {
        // Arrange
        var content = """
            # Comentário
            TEST_VAR_ONE=value1
            TEST_VAR_TWO="value 2 with spaces"
            TEST_VAR_THREE='value 3 with quotes'

            SMTP_HOST=smtp.customhost.com
            SMTP_USERNAME=user@test.com
            """;
        File.WriteAllText(_tempEnvPath, content);

        // Act
        EnvLoader.LoadFromFile(_tempEnvPath);

        // Assert
        Environment.GetEnvironmentVariable("TEST_VAR_ONE").Should().Be("value1");
        Environment.GetEnvironmentVariable("TEST_VAR_TWO").Should().Be("value 2 with spaces");
        Environment.GetEnvironmentVariable("TEST_VAR_THREE").Should().Be("value 3 with quotes");
        Environment.GetEnvironmentVariable("SMTP_HOST").Should().Be("smtp.customhost.com");
        Environment.GetEnvironmentVariable("SMTP_USERNAME").Should().Be("user@test.com");

        // Verificação de aliases automáticos do ASP.NET Core
        Environment.GetEnvironmentVariable("Smtp__Host").Should().Be("smtp.customhost.com");
        Environment.GetEnvironmentVariable("Smtp__Username").Should().Be("user@test.com");
    }

    [Fact]
    public void FindEnvFile_ShouldFindExistingEnvFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var envFile = Path.Combine(tempDir, ".env");
        File.WriteAllText(envFile, "SMTP_HOST=localhost");

        try
        {
            // Act
            var foundPath = EnvLoader.FindEnvFile(tempDir);

            // Assert
            foundPath.Should().NotBeNull();
            foundPath.Should().Be(envFile);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
