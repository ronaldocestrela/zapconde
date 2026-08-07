namespace Modules.AIEngine.Domain.Enums;

/// <summary>
/// Provedores de modelos LLM suportados pela Engine de IA / Semantic Kernel
/// </summary>
public enum AiProvider
{
    /// <summary>
    /// API nativa da OpenAI (gpt-4o, gpt-4o-mini, text-embedding-3-small, etc.)
    /// </summary>
    OpenAI = 1,

    /// <summary>
    /// Serviço Microsoft Azure OpenAI
    /// </summary>
    AzureOpenAI = 2,

    /// <summary>
    /// Provedor de simulação local para testes e desenvolvimento offline sem custos de API
    /// </summary>
    MockLocal = 3
}
