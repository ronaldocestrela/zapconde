using System.Text.RegularExpressions;

namespace Modules.AIEngine.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de chunking semântico de texto.
/// Particiona documentos em blocos mantendo a coesão de parágrafos e frases.
/// </summary>
public class TextChunkerService : ITextChunkerService
{
    public IReadOnlyList<string> ChunkText(string text, int maxChunkSize = 800, int overlap = 150)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<string>();

        var normalizedText = text.Replace("\r\n", "\n").Trim();
        if (normalizedText.Length <= maxChunkSize)
            return new List<string> { normalizedText };

        var chunks = new List<string>();
        // Divide em parágrafos primeiro
        var paragraphs = normalizedText.Split(new[] { "\n\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        var currentChunk = new List<string>();
        var currentLength = 0;

        foreach (var paragraph in paragraphs)
        {
            var pTrim = paragraph.Trim();
            if (string.IsNullOrWhiteSpace(pTrim))
                continue;

            if (currentLength + pTrim.Length + 1 <= maxChunkSize)
            {
                currentChunk.Add(pTrim);
                currentLength += pTrim.Length + 1;
            }
            else
            {
                if (currentChunk.Count > 0)
                {
                    var chunkContent = string.Join(" ", currentChunk);
                    chunks.Add(chunkContent);

                    // Prepara sobreposição
                    var overlapContent = GetOverlapText(chunkContent, overlap);
                    currentChunk.Clear();
                    if (!string.IsNullOrWhiteSpace(overlapContent))
                    {
                        currentChunk.Add(overlapContent);
                        currentLength = overlapContent.Length + 1;
                    }
                    else
                    {
                        currentLength = 0;
                    }
                }

                // Se um único parágrafo for maior que maxChunkSize, corta em frases
                if (pTrim.Length > maxChunkSize)
                {
                    var sentences = Regex.Split(pTrim, @"(?<=[.!?])\s+");
                    foreach (var sentence in sentences)
                    {
                        var sTrim = sentence.Trim();
                        if (string.IsNullOrWhiteSpace(sTrim)) continue;

                        if (currentLength + sTrim.Length + 1 <= maxChunkSize)
                        {
                            currentChunk.Add(sTrim);
                            currentLength += sTrim.Length + 1;
                        }
                        else
                        {
                            if (currentChunk.Count > 0)
                            {
                                var subChunk = string.Join(" ", currentChunk);
                                chunks.Add(subChunk);
                                var subOverlap = GetOverlapText(subChunk, overlap);
                                currentChunk.Clear();
                                if (!string.IsNullOrWhiteSpace(subOverlap))
                                {
                                    currentChunk.Add(subOverlap);
                                    currentLength = subOverlap.Length + 1;
                                }
                                else
                                {
                                    currentLength = 0;
                                }
                            }
                            currentChunk.Add(sTrim);
                            currentLength = sTrim.Length + 1;
                        }
                    }
                }
                else
                {
                    currentChunk.Add(pTrim);
                    currentLength += pTrim.Length + 1;
                }
            }
        }

        if (currentChunk.Count > 0)
        {
            var finalChunk = string.Join(" ", currentChunk);
            if (!chunks.Contains(finalChunk))
            {
                chunks.Add(finalChunk);
            }
        }

        return chunks;
    }

    private static string GetOverlapText(string text, int overlapSize)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= overlapSize)
            return text;

        var sub = text.Substring(text.Length - overlapSize);
        var firstSpace = sub.IndexOf(' ');
        if (firstSpace > 0 && firstSpace < sub.Length - 1)
        {
            return sub.Substring(firstSpace + 1);
        }
        return sub;
    }
}
