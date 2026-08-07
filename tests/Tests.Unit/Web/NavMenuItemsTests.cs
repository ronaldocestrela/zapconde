using FluentAssertions;
using Modules.Identity.Domain;
using SmartCondo.Web.Components.Layout;

namespace Tests.Unit.Web;

public class NavMenuItemsTests
{
    [Theory]
    [InlineData(SmartCondoRoles.Sindico, new[] { "Dashboard", "Unidades e Moradores", "Financeiro", "Simulador Financeiro", "Acordos de Renegociação", "Inadimplência e Régua", "Prestação de Contas", "Conciliação Bancária", "Relatórios Consolidados", "Áreas Comuns", "Reservas", "Ocorrências e Chamados", "Manutenção Preventiva", "Assembleias Virtuais", "Portaria", "WhatsApp / Gateway", "Semantic Kernel / IA", "Configurações" })]
    [InlineData(SmartCondoRoles.Administradora, new[] { "Dashboard", "Unidades e Moradores", "Financeiro", "Simulador Financeiro", "Acordos de Renegociação", "Inadimplência e Régua", "Prestação de Contas", "Conciliação Bancária", "Relatórios Consolidados", "Áreas Comuns", "Reservas", "Ocorrências e Chamados", "Manutenção Preventiva", "Assembleias Virtuais", "Portaria", "WhatsApp / Gateway", "Semantic Kernel / IA", "Configurações" })]
    [InlineData(SmartCondoRoles.Condomino, new[] { "Dashboard", "Financeiro", "Simulador Financeiro", "Acordos de Renegociação", "Prestação de Contas", "Áreas Comuns", "Reservas", "Ocorrências e Chamados", "Assembleias Virtuais", "Portaria", "Configurações" })]
    [InlineData(SmartCondoRoles.Portaria, new[] { "Dashboard", "Portaria", "Configurações" })]
    public void GetItemsForRole_Should_ReturnExpectedItems(string role, string[] expectedLabels)
    {
        var labels = NavMenuItems.GetItemsForRole(role).Select(i => i.Label).ToArray();
        labels.Should().BeEquivalentTo(expectedLabels, options => options.WithStrictOrdering());
    }

    [Fact]
    public void GetItemsForRole_Should_ReturnEmpty_WhenRoleIsNull()
    {
        NavMenuItems.GetItemsForRole(null).Should().BeEmpty();
    }

    [Fact]
    public void GetFooterItemForRole_Should_ReturnSettings()
    {
        var footer = NavMenuItems.GetFooterItemForRole(SmartCondoRoles.Sindico);
        footer.Should().NotBeNull();
        footer!.Href.Should().Be("/configuracoes");
        footer.IsFooter.Should().BeTrue();
    }

    [Fact]
    public void GetGroupedMainItemsForRole_Should_ExcludeFooter()
    {
        var grouped = NavMenuItems.GetGroupedMainItemsForRole(SmartCondoRoles.Sindico).SelectMany(g => g).ToList();
        grouped.Should().NotContain(i => i.IsFooter);
        grouped.Should().Contain(i => i.Label == "Dashboard");
    }

    [Theory]
    [InlineData(SmartCondoRoles.Condomino, "Unidades e Moradores")]
    [InlineData(SmartCondoRoles.Condomino, "Semantic Kernel / IA")]
    [InlineData(SmartCondoRoles.Portaria, "Financeiro")]
    public void GetItemsForRole_Should_ExcludeUnauthorizedItems(string role, string forbiddenLabel)
    {
        NavMenuItems.GetItemsForRole(role).Select(i => i.Label).Should().NotContain(forbiddenLabel);
    }
}
