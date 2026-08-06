using Modules.Financial.Domain.Enums;

namespace Modules.Financial.Application.DTOs;

public record CriarPastaDigitalRequestDto(
    int CondoId,
    int Ano,
    int Mes,
    decimal SaldoAnterior = 0,
    string ResumoIa = "");

public record AdicionarItemBalanceteRequestDto(
    TipoLancamentoBalancete TipoLancamento,
    CategoriaPlanoContas Categoria,
    string Descricao,
    decimal ValorOrcado,
    decimal ValorRealizado,
    DateTime DataLancamento,
    bool Conciliado = false);

public record AnexarDocumentoRequestDto(
    CategoriaDocumentoPrestacao Categoria,
    string Titulo,
    string NomeArquivo,
    string UrlArquivo,
    string ContentType,
    long TamanhoBytes,
    int UploadPorUserId);

public record AprovarPastaDigitalRequestDto(
    int AprovadoPorUserId,
    string Parecer = "");

public record RejeitarPastaDigitalRequestDto(
    string ParecerMotivo);

public record DocumentoPrestacaoDto(
    int Id,
    int PastaDigitalId,
    CategoriaDocumentoPrestacao Categoria,
    string Titulo,
    string NomeArquivo,
    string UrlArquivo,
    string ContentType,
    long TamanhoBytes,
    DateTime DataUpload);

public record ItemBalanceteDto(
    int Id,
    int PastaDigitalId,
    TipoLancamentoBalancete TipoLancamento,
    CategoriaPlanoContas Categoria,
    string Descricao,
    decimal ValorOrcado,
    decimal ValorRealizado,
    DateTime DataLancamento,
    bool Conciliado);

public record PastaDigitalDto(
    int Id,
    int TenantId,
    int CondoId,
    int Ano,
    int Mes,
    StatusPastaDigital Status,
    DateTime DataCriacao,
    DateTime? DataFechamento,
    DateTime? DataAprovacao,
    int? AprovadoPorUserId,
    string ObservacoesConselho,
    string ResumoExecutivoIa,
    decimal SaldoAnterior,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal SaldoMes,
    decimal SaldoAcumulado,
    List<DocumentoPrestacaoDto> Documentos,
    List<ItemBalanceteDto> ItensBalancete);
