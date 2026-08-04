namespace Modules.Identity.Domain;

public class ConfiguracoesIniciais
{
    public int DiaVencimento { get; set; } = 10;

    public bool JurosEnabled { get; set; }

    public bool MultaEnabled { get; set; }

    public BankGateway BankGateway { get; set; } = BankGateway.None;

    public bool WhatsAppAiEnabled { get; set; }

    public static void Validate(int diaVencimento)
    {
        if (diaVencimento is < 1 or > 31)
        {
            throw new DomainValidationException("Dia de vencimento deve estar entre 1 e 31.");
        }
    }
}
