using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.SemanticKernel;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AccessControl.Domain.Enums;

namespace Modules.AIEngine.Application.Plugins;

/// <summary>
/// Plugin do Microsoft.SemanticKernel (Function Calling / Tools) para liberação de visitantes e prestadores de serviço na portaria.
/// </summary>
public class PortariaPlugin
{
    private readonly IVisitanteApplicationService _visitanteService;
    private readonly ICurrentTenantService _currentTenantService;

    public PortariaPlugin(
        IVisitanteApplicationService visitanteService,
        ICurrentTenantService currentTenantService)
    {
        _visitanteService = visitanteService ?? throw new ArgumentNullException(nameof(visitanteService));
        _currentTenantService = currentTenantService ?? throw new ArgumentNullException(nameof(currentTenantService));
    }

    [KernelFunction("AuthorizeGuest")]
    [Description("Registra pré-autorização de liberação de visitante ou prestador de serviço na portaria do condomínio informando nome, documento (CPF/RG), tipo de visitante e data/hora de validade.")]
    public async Task<string> AuthorizeGuestAsync(
        [Description("Nome completo do visitante ou prestador de serviço")] string nome,
        [Description("Número do documento de identificação (CPF ou RG)")] string documento,
        [Description("Data e hora de início da autorização no formato ISO 'yyyy-MM-dd HH:mm' (ex: 2026-09-20 14:00)")] string? dataInicio = null,
        [Description("Data e hora de término/validade da autorização no formato ISO 'yyyy-MM-dd HH:mm' (ex: 2026-09-20 18:00)")] string? dataFim = null,
        [Description("Tipo do visitante: 'Visitante' ou 'PrestadorServico' (padrão: Visitante)")] string tipo = "Visitante",
        [Description("ID numérico da unidade residencial de destino (ex: 102)")] int unidadeId = 1,
        [Description("Identificação do bloco e unidade (ex: 'Bloco A - Apto 102')")] string? blocoUnidade = null,
        [Description("ID numérico do morador solicitante (opcional)")] int? moradorId = null,
        [Description("Telefone de contato do visitante (opcional)")] string? telefone = null,
        [Description("Nome da empresa / razão social (obrigatório se tipo for 'PrestadorServico')")] string? empresa = null,
        [Description("Placa do veículo do visitante para cadastro na portaria (opcional)")] string? placaVeiculo = null,
        [Description("Observações adicionais para a portaria (opcional)")] string? observacoes = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = "O nome completo do visitante é obrigatório para autorização."
            });
        }

        if (string.IsNullOrWhiteSpace(documento))
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = "O documento (CPF/RG) é obrigatório para autorização na portaria."
            });
        }

        var tipoEnum = TipoVisitante.VisitanteSocial;
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            if (tipo.Equals("Visitante", StringComparison.OrdinalIgnoreCase))
            {
                tipoEnum = TipoVisitante.VisitanteSocial;
            }
            else if (Enum.TryParse<TipoVisitante>(tipo, true, out var parsedTipo))
            {
                tipoEnum = parsedTipo;
            }
        }

        if (tipoEnum == TipoVisitante.PrestadorServico && string.IsNullOrWhiteSpace(empresa))
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = "O nome da empresa / razão social é obrigatório ao autorizar um Prestador de Serviço."
            });
        }

        DateTimeOffset? dtInicio = null;
        if (!string.IsNullOrWhiteSpace(dataInicio) && TryParseDateTime(dataInicio, out var parsedInicio))
        {
            dtInicio = new DateTimeOffset(parsedInicio, TimeSpan.Zero);
        }

        DateTimeOffset? dtFim = null;
        if (!string.IsNullOrWhiteSpace(dataFim) && TryParseDateTime(dataFim, out var parsedFim))
        {
            dtFim = new DateTimeOffset(parsedFim, TimeSpan.Zero);
        }

        var targetUnidadeId = unidadeId > 0 ? unidadeId : 1;
        var targetBloco = string.IsNullOrWhiteSpace(blocoUnidade) ? $"Unidade {targetUnidadeId}" : blocoUnidade.Trim();

        var request = new CreateVisitanteRequestDto(
            NomeCompleto: nome.Trim(),
            Documento: documento.Trim(),
            Telefone: telefone?.Trim(),
            Tipo: tipoEnum,
            UnidadeId: targetUnidadeId,
            BlocoUnidade: targetBloco,
            MoradorId: moradorId,
            DataHoraInicioAutorizacao: dtInicio ?? DateTimeOffset.UtcNow,
            DataHoraFimAutorizacao: dtFim ?? DateTimeOffset.UtcNow.AddDays(1),
            Empresa: empresa?.Trim(),
            PlacaVeiculo: placaVeiculo?.Trim().ToUpperInvariant(),
            Observacoes: observacoes?.Trim() ?? "Autorização registrada via Assistente Virtual (IA)",
            RegistrarEntradaImediata: false
        );

        var result = await _visitanteService.AuthorizeVisitanteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? string.Join("; ", result.Errors ?? Array.Empty<string>())
            });
        }

        var v = result.Data!;

        var responseObj = new
        {
            sucesso = true,
            autorizacaoId = v.Id,
            nomeCompleto = v.NomeCompleto,
            documento = v.Documento,
            tipo = v.Tipo.ToString(),
            status = v.Status.ToString(),
            unidadeId = v.UnidadeId,
            blocoUnidade = v.BlocoUnidade,
            empresa = v.Empresa,
            placaVeiculo = v.PlacaVeiculo,
            validadeInicio = v.DataHoraInicioAutorizacao?.ToString("yyyy-MM-dd HH:mm"),
            validadeFim = v.DataHoraFimAutorizacao?.ToString("yyyy-MM-dd HH:mm"),
            mensagem = $"Liberação de {v.Tipo} '{v.NomeCompleto}' registrada na portaria para a {v.BlocoUnidade} com código #{v.Id}."
        };

        return JsonSerializer.Serialize(responseObj, new JsonSerializerOptions { WriteIndented = false });
    }

    private static bool TryParseDateTime(string input, out DateTime result)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            result = default;
            return false;
        }

        var formats = new[]
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HH:mm:ss"
        };

        if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result))
        {
            return true;
        }

        return DateTime.TryParse(input, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);
    }
}
