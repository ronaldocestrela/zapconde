using BuildingBlocks.Shared;
using Modules.Identity.Application.Dtos;

namespace Modules.Identity.Application;

public interface IPhoneVerificationService
{
    Task<Result<PhoneVerificationStatusDto>> RequestCodeAsync(
        int moradorId,
        string phoneNumber,
        bool isResend,
        CancellationToken ct = default);

    Task<Result<PhoneVerificationStatusDto>> VerifyAsync(
        int moradorId,
        string code,
        CancellationToken ct = default);

    Task<Result<PhoneVerificationStatusDto>> GetStatusAsync(
        int moradorId,
        CancellationToken ct = default);
}

public interface IWhatsAppOtpSender
{
    Task SendOtpAsync(string phoneNumberE164, string code, CancellationToken ct = default);
}
