namespace Modules.Identity.Domain;

/// <summary>
/// Estado do vínculo entre um morador e seu celular WhatsApp.
/// </summary>
public enum PhoneVerificationStatus
{
    NaoInformado = 0,
    AguardandoValidacao = 1,
    Validado = 2,
    Expirado = 3
}
