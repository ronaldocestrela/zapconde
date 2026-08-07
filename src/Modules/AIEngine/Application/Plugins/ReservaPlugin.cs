using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using BuildingBlocks.Shared.MultiTenancy;
using Microsoft.SemanticKernel;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.AIEngine.Application.Plugins;

/// <summary>
/// Plugin do Microsoft.SemanticKernel (Function Calling / Tools) para agendamento e validação de áreas comuns.
/// </summary>
public class ReservaPlugin
{
    private readonly IReservaApplicationService _reservaService;
    private readonly IAreaComumApplicationService _areaComumService;
    private readonly ICurrentTenantService _currentTenantService;

    public ReservaPlugin(
        IReservaApplicationService reservaService,
        IAreaComumApplicationService areaComumService,
        ICurrentTenantService currentTenantService)
    {
        _reservaService = reservaService ?? throw new ArgumentNullException(nameof(reservaService));
        _areaComumService = areaComumService ?? throw new ArgumentNullException(nameof(areaComumService));
        _currentTenantService = currentTenantService ?? throw new ArgumentNullException(nameof(currentTenantService));
    }

    [KernelFunction("ReserveCommonArea")]
    [Description("Valida e realiza o agendamento/reserva de uma área comum do condomínio (ex: Salão de Festas, Churrasqueira) para um determinado morador informando areaId, dataInicio, dataFim, moradorId e quantidade de pessoas.")]
    public async Task<string> ReserveCommonAreaAsync(
        [Description("ID numérico da área comum cadastrada no condomínio (area_id)")] int areaId,
        [Description("Data e hora de início da reserva no formato ISO 'yyyy-MM-dd HH:mm' (ex: 2026-09-15 18:00)")] string dataInicio,
        [Description("Data e hora de término da reserva no formato ISO 'yyyy-MM-dd HH:mm' (ex: 2026-09-15 22:00)")] string dataFim,
        [Description("ID numérico do morador solicitante (morador_id)")] int moradorId,
        [Description("Quantidade estimada de pessoas/convidados (padrão: 1)")] int quantidadePessoas = 1,
        [Description("Observação ou descrição do evento (opcional)")] string? observacao = null,
        CancellationToken cancellationToken = default)
    {
        if (areaId <= 0)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = "ID da área comum inválido para agendamento."
            });
        }

        if (moradorId <= 0)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = "ID do morador inválido para agendamento."
            });
        }

        if (!TryParseDateTime(dataInicio, out var dtInicio))
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = $"Formato da data/hora de início inválido: '{dataInicio}'. Utilize 'yyyy-MM-dd HH:mm' ou 'yyyy-MM-ddTHH:mm:ss'."
            });
        }

        if (!TryParseDateTime(dataFim, out var dtFim))
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = $"Formato da data/hora de término inválido: '{dataFim}'. Utilize 'yyyy-MM-dd HH:mm' ou 'yyyy-MM-ddTHH:mm:ss'."
            });
        }

        var condoId = _currentTenantService.CondoId ?? 1;

        var request = new CreateReservaRequest(
            CondoId: condoId,
            AreaComumId: areaId,
            MoradorId: moradorId,
            NomeMorador: $"Morador #{moradorId}",
            UnidadeMorador: "Unidade Cadastrada",
            DataInicio: dtInicio,
            DataFim: dtFim,
            QuantidadePessoas: quantidadePessoas,
            Observacao: observacao ?? "Agendamento realizado via Assistente Virtual (IA)"
        );

        var result = await _reservaService.CriarReservaAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? string.Join("; ", result.Errors ?? Array.Empty<string>())
            });
        }

        var res = result.Data!;

        var responseObj = new
        {
            sucesso = true,
            reservaId = res.Id,
            areaId = res.AreaComumId,
            nomeAreaComum = res.NomeAreaComum,
            moradorId = res.MoradorId,
            dataInicio = res.DataInicio.ToString("yyyy-MM-dd HH:mm"),
            dataFim = res.DataFim.ToString("yyyy-MM-dd HH:mm"),
            quantidadePessoas = res.QuantidadePessoas,
            status = res.Status.ToString(),
            valorReserva = res.ValorTaxaReserva,
            valorLimpeza = res.ValorTaxaLimpeza,
            valorTotal = res.ValorTotal,
            mensagem = res.Status == StatusReserva.Confirmada
                ? $"Reserva #{res.Id} da área '{res.NomeAreaComum}' confirmada com sucesso!"
                : $"Solicitação de reserva #{res.Id} registrada e aguardando aprovação do síndico."
        };

        return JsonSerializer.Serialize(responseObj, new JsonSerializerOptions { WriteIndented = false });
    }

    [KernelFunction("GetAvailableCommonAreas")]
    [Description("Retorna a lista de áreas comuns ativas no condomínio contendo nome, capacidade máxima, taxas de uso e horários de funcionamento.")]
    public async Task<string> GetAvailableCommonAreasAsync(
        [Description("ID do condomínio (opcional, padrão 1)")] int condoId = 1,
        CancellationToken cancellationToken = default)
    {
        var targetCondoId = condoId > 0 ? condoId : (_currentTenantService.CondoId ?? 1);
        var result = await _areaComumService.GetAllAsync(targetCondoId, StatusAreaComum.Ativa, null, cancellationToken);

        if (!result.IsSuccess)
        {
            return JsonSerializer.Serialize(new
            {
                sucesso = false,
                mensagem = result.Message ?? "Erro ao listar áreas comuns."
            });
        }

        var list = result.Data?.ToList() ?? new List<AreaComumDto>();

        var responseObj = new
        {
            sucesso = true,
            totalAreas = list.Count,
            areas = list.Select(a => new
            {
                id = a.Id,
                nome = a.Nome,
                descricao = a.Descricao,
                capacidadeMaxima = a.CapacidadeMaxima,
                taxaReserva = a.TaxaReserva,
                taxaLimpeza = a.TaxaLimpeza,
                horarioInicio = a.HorarioInicioFuncionamento,
                horarioFim = a.HorarioFimFuncionamento,
                requerAprovacao = a.RequerAprovacaoSindico
            })
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
