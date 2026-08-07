using Microsoft.SemanticKernel;
using Modules.AIEngine.Domain.Entities;
using Modules.AIEngine.Domain.Enums;
using Modules.AIEngine.Domain.Exceptions;

namespace Modules.AIEngine.Application.Services;

public class AiKernelFactory : IAiKernelFactory
{
    public Kernel CreateKernel(AiKernelConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        if (!config.IsActive)
            throw new AiEngineDomainException("A configuração do Semantic Kernel para este condomínio está inativa.");

        var builder = Kernel.CreateBuilder();

        switch (config.Provider)
        {
            case AiProvider.OpenAI:
                if (string.IsNullOrWhiteSpace(config.ApiKey))
                    throw new AiEngineDomainException("A chave de API (ApiKey) da OpenAI não foi configurada.");

                builder.AddOpenAIChatCompletion(
                    modelId: config.ModelId,
                    apiKey: config.ApiKey,
                    orgId: string.IsNullOrWhiteSpace(config.OrgId) ? null : config.OrgId);
                break;

            case AiProvider.AzureOpenAI:
                if (string.IsNullOrWhiteSpace(config.ApiKey))
                    throw new AiEngineDomainException("A chave de API da Azure OpenAI não foi configurada.");

                if (string.IsNullOrWhiteSpace(config.Endpoint))
                    throw new AiEngineDomainException("O Endpoint da Azure OpenAI é obrigatório.");

                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: config.ModelId,
                    endpoint: config.Endpoint,
                    apiKey: config.ApiKey);
                break;

            case AiProvider.MockLocal:
                // Instância básica do Kernel sem conectores externos para desenvolvimento/teste offline
                break;

            default:
                throw new AiEngineDomainException($"Provedor de IA '{config.Provider}' não suportado.");
        }

        return builder.Build();
    }
}
