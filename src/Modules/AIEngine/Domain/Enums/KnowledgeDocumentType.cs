namespace Modules.AIEngine.Domain.Enums;

/// <summary>
/// Tipo de documento cadastrado na base de conhecimento RAG.
/// </summary>
public enum KnowledgeDocumentType
{
    RegimentoInterno = 1,
    ConvencaoCondominial = 2,
    RegulamentoAreaComum = 3,
    ManualCondomino = 4,
    Outros = 99
}
