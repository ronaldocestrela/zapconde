namespace Modules.Financial.Domain.Enums;

/// <summary>
/// Tipo de ação executada na régua de inadimplência.
/// </summary>
public enum TipoAcaoCobranca
{
    LembreteAmigavel = 1,
    NotificacaoCobranca = 2,
    PropostaAcordo = 3,
    EncaminhamentoJuridico = 4
}
