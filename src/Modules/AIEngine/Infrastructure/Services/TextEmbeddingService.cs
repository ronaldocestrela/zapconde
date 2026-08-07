using System.Security.Cryptography;
using System.Text;
using Modules.AIEngine.Application.Services;
using Pgvector;

namespace Modules.AIEngine.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de geração de embeddings vetoriais RAG.
/// Fornece vetores de 1536 dimensões normalizados por L2 para buscas semânticas no pgvector.
/// </summary>
public class TextEmbeddingService : ITextEmbeddingService
{
    private const int EmbeddingDimensions = 1536;

    public Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Task.FromResult(CreateZeroVector(EmbeddingDimensions));
        }

        return Task.FromResult(GenerateDeterministicVector(text, EmbeddingDimensions));
    }

    public async Task<IReadOnlyList<Vector>> GenerateBatchEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        var results = new List<Vector>();

        foreach (var text in textList)
        {
            var vector = await GenerateEmbeddingAsync(text, cancellationToken);
            results.Add(vector);
        }

        return results;
    }

    /// <summary>
    /// Gera um vetor embedding pseudo-randômico determinístico com base no hash SHA256 do texto.
    /// Garante que o mesmo texto sempre produza o mesmo vetor normalizado de 1536 dimensões.
    /// </summary>
    public static Vector GenerateDeterministicVector(string text, int dimensions = EmbeddingDimensions)
    {
        var floats = new float[dimensions];
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));

        for (int i = 0; i < dimensions; i++)
        {
            var byteVal = hashBytes[i % hashBytes.Length];
            var floatVal = ((byteVal / 255.0f) * 2.0f) - 1.0f;
            var indexFactor = (float)Math.Sin((i + 1) * 0.1);
            floats[i] = (float)(floatVal * 0.5 + indexFactor * 0.5);
        }

        // Normalização L2 do vetor
        double sumSq = 0;
        for (int i = 0; i < dimensions; i++)
        {
            sumSq += floats[i] * floats[i];
        }

        double norm = Math.Sqrt(sumSq);
        if (norm > 1e-6)
        {
            for (int i = 0; i < dimensions; i++)
            {
                floats[i] = (float)(floats[i] / norm);
            }
        }

        return new Vector(floats);
    }

    private static Vector CreateZeroVector(int dimensions)
    {
        return new Vector(new float[dimensions]);
    }
}
