namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Categoria de documento anexo à pasta de prestação de contas.
/// </summary>
public enum CategoriaDocumentoPrestacao
{
    ExtratoBancario = 1,
    NotaFiscalDespesa = 2,
    ComprovantePagamento = 3,
    RelatorioInadimplencia = 4,
    Outros = 99
}
