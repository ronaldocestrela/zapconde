using System.Text.Json;
using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AIEngine.Application.Plugins;
using Modules.Operations.Application.DTOs;
using Modules.Operations.Application.Services;
using Modules.Operations.Domain.Enums;

namespace Modules.AIEngine.Endpoints;

public record ExecuteReservaPluginRequest(
    int AreaId,
    string DataInicio,
    string DataFim,
    int MoradorId,
    int QuantidadePessoas = 1,
    string? Observacao = null);

public record ReservaPluginExecutionResultDto(
    int AreaId,
    string NomeAreaComum,
    int MoradorId,
    int? ReservaId,
    string DataInicio,
    string DataFim,
    int QuantidadePessoas,
    string Status,
    decimal ValorTotal,
    bool Sucesso,
    string Mensagem,
    string MensagemFormatadaIa);

/// <summary>
/// Endpoint para simular a invocação da tool ReserveCommonArea do ReservaPlugin (Semantic Kernel Function Calling).
/// </summary>
public sealed class ExecuteReservaPluginEndpoint : Endpoint<ExecuteReservaPluginRequest, Result<ReservaPluginExecutionResultDto>>
{
    private readonly ReservaPlugin _reservaPlugin;
    private readonly IAreaComumApplicationService _areaComumService;

    public ExecuteReservaPluginEndpoint(ReservaPlugin reservaPlugin, IAreaComumApplicationService areaComumService)
    {
        _reservaPlugin = reservaPlugin;
        _areaComumService = areaComumService;
    }

    public override void Configure()
    {
        Post("/api/ai/plugins/reservas/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Executar Function Calling do ReservaPlugin (ReserveCommonArea)";
            s.Description = "Invoca a tool ReserveCommonArea do Semantic Kernel simulando a resposta em linguagem natural e estruturada do agendamento.";
        });
    }

    public override async Task HandleAsync(ExecuteReservaPluginRequest req, CancellationToken ct)
    {
        if (req.AreaId <= 0 || req.MoradorId <= 0)
        {
            await SendAsync(Result<ReservaPluginExecutionResultDto>.ValidationFailure(["AreaId e MoradorId são obrigatórios e devem ser maiores que zero."]), 400, ct);
            return;
        }

        var jsonResponse = await _reservaPlugin.ReserveCommonAreaAsync(
            req.AreaId,
            req.DataInicio,
            req.DataFim,
            req.MoradorId,
            req.QuantidadePessoas,
            req.Observacao,
            ct);

        using var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;
        var sucesso = root.TryGetProperty("sucesso", out var sucProp) && sucProp.GetBoolean();
        var mensagem = root.TryGetProperty("mensagem", out var msgProp) ? msgProp.GetString() ?? string.Empty : string.Empty;

        string nomeArea = "Área Comum";
        int? reservaId = null;
        string status = "Desconhecido";
        decimal valorTotal = 0.00m;

        if (sucesso)
        {
            if (root.TryGetProperty("nomeAreaComum", out var nProp)) nomeArea = nProp.GetString() ?? nomeArea;
            if (root.TryGetProperty("reservaId", out var rProp)) reservaId = rProp.GetInt32();
            if (root.TryGetProperty("status", out var sProp)) status = sProp.GetString() ?? status;
            if (root.TryGetProperty("valorTotal", out var vProp)) valorTotal = vProp.GetDecimal();
        }
        else
        {
            var areaRes = await _areaComumService.GetByIdAsync(req.AreaId, ct);
            if (areaRes.IsSuccess && areaRes.Data != null)
            {
                nomeArea = areaRes.Data.Nome;
            }
        }

        var mensagemFormatadaIa = sucesso
            ? $"Olá! A reserva #{reservaId} para a área '{nomeArea}' foi agendada com sucesso para {req.DataInicio} até {req.DataFim} ({req.QuantidadePessoas} pessoa(s)). Status: {status}. Valor Total: R$ {valorTotal:F2}."
            : $"Atenção: Não foi possível agendar a área '{nomeArea}'. Motivo: {mensagem}";

        var resultDto = new ReservaPluginExecutionResultDto(
            AreaId: req.AreaId,
            NomeAreaComum: nomeArea,
            MoradorId: req.MoradorId,
            ReservaId: reservaId,
            DataInicio: req.DataInicio,
            DataFim: req.DataFim,
            QuantidadePessoas: req.QuantidadePessoas,
            Status: status,
            ValorTotal: valorTotal,
            Sucesso: sucesso,
            Mensagem: mensagem,
            MensagemFormatadaIa: mensagemFormatadaIa);

        var httpStatus = sucesso ? 200 : 400;
        await SendAsync(Result<ReservaPluginExecutionResultDto>.Success(resultDto, mensagem), httpStatus, ct);
    }
}

public record ListActiveAreasRequest(int CondoId = 1);

/// <summary>
/// Endpoint para consultar as áreas comuns ativas do condomínio para consumo da UI de simulador de plugins.
/// </summary>
public sealed class ListActiveAreasEndpoint : Endpoint<ListActiveAreasRequest, Result<IEnumerable<AreaComumDto>>>
{
    private readonly IAreaComumApplicationService _areaComumService;

    public ListActiveAreasEndpoint(IAreaComumApplicationService areaComumService) => _areaComumService = areaComumService;

    public override void Configure()
    {
        Get("/api/ai/plugins/reservas/areas");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Listar Áreas Comuns Ativas";
            s.Description = "Retorna o catálogo de áreas comuns ativas registradas no condomínio para seleção no simulador de IA.";
        });
    }

    public override async Task HandleAsync(ListActiveAreasRequest req, CancellationToken ct)
    {
        var result = await _areaComumService.GetAllAsync(req.CondoId, StatusAreaComum.Ativa, null, ct);
        var statusCode = result.IsSuccess ? 200 : 400;
        await SendAsync(result, statusCode, ct);
    }
}
