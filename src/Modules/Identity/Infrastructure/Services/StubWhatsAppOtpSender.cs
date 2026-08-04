using Microsoft.Extensions.Logging;
using Modules.Identity.Application;

namespace Modules.Identity.Infrastructure.Services;

public sealed class StubWhatsAppOtpSender(ILogger<StubWhatsAppOtpSender> logger) : IWhatsAppOtpSender
{
    public Task SendOtpAsync(string phoneNumberE164, string code, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Envio OTP WhatsApp simulado para {MaskedPhone}. Integração BSP será ativada na Fase 6.",
            Mask(phoneNumberE164));
        return Task.CompletedTask;
    }

    private static string Mask(string phone) =>
        phone.Length < 6 ? "***" : $"{phone[..3]}*******{phone[^4..]}";
}
