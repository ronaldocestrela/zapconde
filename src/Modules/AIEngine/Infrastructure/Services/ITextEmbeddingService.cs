using Pgvector;

namespace Modules.AIEngine.Infrastructure.Services;

/// <summary>
/// Serviço de geração de embeddings vetoriais para textos (RAG).
/// </summary>
public interface ITextEmbeddingService
{
    /// <summary>
    /// Gera o vetor embedding de 1536 dimensões para um determinado texto.
    /// </summary>
    /// <param name="text">Texto para conversão vetorial</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Vetor embedding de 1536 dimensões do pgvector</returns>
    Task<Vector> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera embeddings vetoriais em lote para múltiplos trechos de texto.
    /// </summary>
    /// <param name="texts">Coleção de trechos</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de vetores embeddings</returns>
    Task<IReadOnlyList<Vector>> GenerateBatchEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
}
