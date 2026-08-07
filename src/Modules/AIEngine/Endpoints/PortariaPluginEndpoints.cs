using System.Text.Json;
using BuildingBlocks.Shared;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Modules.AccessControl.Application.DTOs;
using Modules.AccessControl.Application.Services;
using Modules.AIEngine.Application.Plugins;

namespace Modules.AIEngine.Endpoints;

public record ExecuteAuthorizeGuestPluginRequest(
    string Nome,
    string Documento,
    string? DataInicio = null,
    string? DataFim = null,
    string Tipo = "Visitante",
    int UnidadeId = 1,
    string? BlocoUnidade = null,
    int? MoradorId = null,
    string? Telefone = null,
    string? Empresa = null,
    string? PlacaVeiculo = null,
    string? Observacoes = null);

public record AuthorizeGuestPluginExecutionResultDto(
    int? AutorizacaoId,
    string NomeCompleto,
    string Documento,
    string Tipo,
    string Status,
    int UnidadeId,
    string BlocoUnidade,
    string? Empresa,
    string? PlacaVeiculo,
    string ValidadeInicio,
    string ValidadeFim,
    bool Sucesso,
    string Mensagem,
    string MensagemFormatadaIa);

/// <summary>
/// Endpoint para simular a invocação da tool AuthorizeGuest do PortariaPlugin (Semantic Kernel Function Calling).
/// </summary>
public sealed class ExecutePortariaPluginEndpoint : Endpoint<ExecuteAuthorizeGuestPluginRequest, Result<AuthorizeGuestPluginExecutionResultDto>>
{
    private readonly PortariaPlugin _portariaPlugin;

    public ExecutePortariaPluginEndpoint(PortariaPlugin portariaPlugin)
    {
        _portariaPlugin = portariaPlugin;
    }

    public override void Configure()
    {
        Post("/api/ai/plugins/portaria/execute");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Executar Function Calling do PortariaPlugin (AuthorizeGuest)";
            s.Description = "Invoca a tool AuthorizeGuest do Semantic Kernel simulando a pré-autorização de visitante/prestador na portaria.";
        });
    }

    public override async Task HandleAsync(ExecuteAuthorizeGuestPluginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Documento))
        {
            await SendAsync(Result<AuthorizeGuestPluginExecutionResultDto>.ValidationFailure(["Nome do visitante e documento (CPF/RG) são obrigatórios."]), 400, ct);
            return;
        }

        var jsonResponse = await _portariaPlugin.AuthorizeGuestAsync(
            req.Nome,
            req.Documento,
            req.DataInicio,
            req.DataFim,
            req.Tipo,
            req.UnidadeId,
            req.BlocoUnidade,
            req.MoradorId,
            req.Telefone,
            req.Empresa,
            req.PlacaVeiculo,
            req.Observacoes,
            ct);

        using var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;
        var sucesso = root.TryGetProperty("sucesso", out var sucProp) && sucProp.GetBoolean();
        var mensagem = root.TryGetProperty("mensagem", out var msgProp) ? msgProp.GetString() ?? string.Empty : string.Empty;

        int? autorizacaoId = null;
        string nomeCompleto = req.Nome;
        string documento = req.Documento;
        string tipo = req.Tipo;
        string status = "Desconhecido";
        int unidadeId = req.UnidadeId;
        string blocoUnidade = req.BlocoUnidade ?? $"Unidade {req.UnidadeId}";
        string? empresa = req.Empresa;
        string? placaVeiculo = req.PlacaVeiculo;
        string validadeInicio = req.DataInicio ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        string validadeFim = req.DataFim ?? DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd HH:mm");

        if (sucesso)
        {
            if (root.TryGetProperty("autorizacaoId", out var aProp)) autorizacaoId = aProp.GetInt32();
            if (root.TryGetProperty("nomeCompleto", out var nProp)) nomeCompleto = nProp.GetString() ?? nomeCompleto;
            if (root.TryGetProperty("documento", out var dProp)) documento = dProp.GetString() ?? documento;
            if (root.TryGetProperty("tipo", out var tProp)) tipo = tProp.GetString() ?? tipo;
            if (root.TryGetProperty("status", out var sProp)) status = sProp.GetString() ?? status;
            if (root.TryGetProperty("blocoUnidade", out var bProp)) blocoUnidade = bProp.GetString() ?? blocoUnidade;
            if (root.TryGetProperty("empresa", out var eProp)) empresa = eProp.GetString();
            if (root.TryGetProperty("placaVeiculo", out var pProp)) placaVeiculo = pProp.GetString();
            if (root.TryGetProperty("validadeInicio", out var viProp)) validadeInicio = viProp.GetString() ?? validadeInicio;
            if (root.TryGetProperty("validadeFim", out var vfProp)) validadeFim = vfProp.GetString() ?? validadeFim;
        }

        var mensagemFormatadaIa = sucesso
            ? $"Olá! A autorização #{autorizacaoId} para o {tipo} '{nomeCompleto}' (Doc: {documento}) foi registrada com sucesso na portaria para a {blocoUnidade}. Validez: de {validadeInicio} até {validadeFim}."
            : $"Atenção: Não foi possível autorizar o ingresso de '{req.Nome}'. Motivo: {mensagem}";

        var resultDto = new AuthorizeGuestPluginExecutionResultDto(
            AutorizacaoId: autorizacaoId,
            NomeCompleto: nomeCompleto,
            Documento: documento,
            Tipo: tipo,
            Status: status,
            UnidadeId: unidadeId,
            BlocoUnidade: blocoUnidade,
            Empresa: empresa,
            PlacaVeiculo: placaVeiculo,
            ValidadeInicio: validadeInicio,
            ValidadeFim: validadeFim,
            Sucesso: sucesso,
            Mensagem: mensagem,
            MensagemFormatadaIa: mensagemFormatadaIa);

        var httpStatus = sucesso ? 200 : 400;
        await SendAsync(Result<AuthorizeGuestPluginExecutionResultDto>.Success(resultDto, mensagem), httpStatus, ct);
    }
}
