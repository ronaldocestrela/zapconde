using System.Reflection;
using BuildingBlocks.Shared.MultiTenancy;
using FluentAssertions;

namespace Tests.Architecture;

public sealed class PhoneVerificationArchitectureTests
{
    [Fact]
    public void PhoneVerificationEndpoints_Should_ReturnResultTypes()
    {
        var assembly = Assembly.Load("Modules.Identity");
        var expected = new[]
        {
            "RequestPhoneVerificationEndpoint",
            "VerifyPhoneEndpoint",
            "ResendPhoneVerificationEndpoint",
            "GetPhoneVerificationStatusEndpoint"
        };

        foreach (var name in expected)
        {
            var endpoint = assembly.GetType($"Modules.Identity.Endpoints.{name}");
            endpoint.Should().NotBeNull(name);
            var responseType = endpoint!.BaseType!.GetGenericArguments().Last();
            responseType.Name.Should().StartWith("Result");
        }
    }

    [Fact]
    public void PhoneVerificationService_Should_ImplementApplicationContract()
    {
        var assembly = Assembly.Load("Modules.Identity");
        var service = assembly.GetType("Modules.Identity.Infrastructure.Services.PhoneVerificationService");
        var contract = assembly.GetType("Modules.Identity.Application.IPhoneVerificationService");

        service.Should().NotBeNull();
        contract.Should().NotBeNull();
        contract!.IsAssignableFrom(service!).Should().BeTrue();
    }

    [Fact]
    public void Morador_Should_RemainTenantScoped()
    {
        typeof(Modules.Identity.Domain.Morador).GetInterfaces()
            .Should().Contain(typeof(ITenantScoped));
    }

    [Fact]
    public void PhoneVerificationUi_Should_ContainStitchE2eTargets()
    {
        var root = FindRepositoryRoot();
        var page = File.ReadAllText(Path.Combine(
            root,
            "src/Web/SmartCondo.Web/Components/Pages/Phone/PhoneVerification.razor"));
        var app = File.ReadAllText(Path.Combine(
            root,
            "src/Web/SmartCondo.Web/Components/App.razor"));

        page.Should().Contain("(00) 00000-0000");
        page.Should().Contain("Aguardando Validação");
        page.Should().Contain("Número Validado & Vinculado");
        page.Should().Contain("Código Expirado");
        page.Should().Contain("maxlength=\"1\"");
        app.Should().Contain("css/stitch/phone.css");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "SmartCondo.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Raiz da solução não encontrada.");
    }
}
