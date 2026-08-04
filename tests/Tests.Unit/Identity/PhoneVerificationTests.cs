using FluentAssertions;
using Modules.Identity.Domain;

namespace Tests.Unit.Identity;

public sealed class PhoneNumberValidatorTests
{
    [Theory]
    [InlineData("(11) 98765-4321", "+5511987654321")]
    [InlineData("55 11 98765-4321", "+5511987654321")]
    [InlineData("+5511987654321", "+5511987654321")]
    public void NormalizeBr_WithValidMobile_Should_ReturnE164(string input, string expected)
    {
        PhoneNumberValidator.TryNormalizeBrazilianMobile(input, out var normalized).Should().BeTrue();
        normalized.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("119999")]
    [InlineData("(11) 18765-4321")]
    [InlineData("+441234567890")]
    public void NormalizeBr_WithInvalidNumber_Should_Fail(string input)
    {
        PhoneNumberValidator.TryNormalizeBrazilianMobile(input, out _).Should().BeFalse();
    }
}

public sealed class MoradorPhoneVerificationTests
{
    [Fact]
    public void PhoneVerification_Should_TransitionFromPendingToVerified()
    {
        var morador = Morador.Create(1, 10, "João Silva", "52998224725", "joao@test.com", "");
        var requestedAt = new DateTime(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);

        morador.IniciarVerificacaoTelefone("+5511987654321", requestedAt);

        morador.PhoneVerificationStatus.Should().Be(PhoneVerificationStatus.AguardandoValidacao);
        morador.TelefoneWhatsAppE164.Should().Be("+5511987654321");
        morador.PhoneVerificationRequestedAtUtc.Should().Be(requestedAt);

        morador.ConfirmarTelefone(requestedAt.AddMinutes(1));

        morador.PhoneVerificationStatus.Should().Be(PhoneVerificationStatus.Validado);
        morador.PhoneVerifiedAtUtc.Should().Be(requestedAt.AddMinutes(1));
    }

    [Fact]
    public void MarkExpired_Should_ClearVerificationTimestamp()
    {
        var morador = Morador.Create(1, 10, "João Silva", "52998224725", "joao@test.com", "");
        morador.IniciarVerificacaoTelefone("+5511987654321", DateTime.UtcNow);

        morador.MarcarCodigoExpirado();

        morador.PhoneVerificationStatus.Should().Be(PhoneVerificationStatus.Expirado);
        morador.PhoneVerifiedAtUtc.Should().BeNull();
    }
}
