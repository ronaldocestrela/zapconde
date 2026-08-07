using Microsoft.SemanticKernel;
using Modules.AIEngine.Domain.Entities;

namespace Modules.AIEngine.Application.Services;

/// <summary>
/// Fábrica para instanciar a estrutura do Microsoft.SemanticKernel com base nas configurações do tenant
/// </summary>
public interface IAiKernelFactory
{
    /// <summary>
    /// Cria uma instância configurada do Kernel do Semantic Kernel com suporte a plugins/tools
    /// </summary>
    Kernel CreateKernel(AiKernelConfig config, IEnumerable<object>? plugins = null);
}

