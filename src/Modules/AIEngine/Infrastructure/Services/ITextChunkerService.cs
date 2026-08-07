namespace Modules.AIEngine.Infrastructure.Services;

/// <summary>
/// Serviço de particionamento (chunking) de textos para o pipeline RAG.
/// </summary>
public interface ITextChunkerService
{
    /// <summary>
    /// Divide um texto em fragmentos (chunks) respeitando limites de tamanho e sobreposição.
    /// </summary>
    /// <param name="text">Texto completo do documento</param>
    /// <param name="maxChunkSize">Tamanho máximo de cada chunk em caracteres</param>
    /// <param name="overlap">Sobreposição de caracteres entre chunks consecutivos</param>
    /// <returns>Lista de fragmentos de texto</returns>
    IReadOnlyList<string> ChunkText(string text, int maxChunkSize = 800, int overlap = 150);
}
