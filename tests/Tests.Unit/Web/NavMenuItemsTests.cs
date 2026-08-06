using FluentAssertions;
using Modules.Identity.Domain;
using SmartCondo.Web.Components.Layout;

namespace Tests.Unit.Web;

public class NavMenuItemsTests
{
    [Theory]
    [InlineData(SmartCondoRoles.Sindico, new[] { "Início", "Unidades e Moradores", "Financeiro", "Simulador Financeiro", "Acordos de Renegociação", "Inadimplência e Régua", "Prestação de Contas", "Conciliação Bancária", "Relatórios Consolidados", "Operações", "Portaria", "WhatsApp / IA", "Configurações" })]
    [InlineData(SmartCondoRoles.Administradora, new[] { "Início", "Unidades e Moradores", "Financeiro", "Simulador Financeiro", "Acordos de Renegociação", "Inadimplência e Régua", "Prestação de Contas", "Conciliação Bancária", "Relatórios Consolidados", "Operações", "Portaria", "WhatsApp / IA", "Configurações" })]
    [InlineData(SmartCondoRoles.Condomino, new[] { "Início", "Financeiro", "Simulador Financeiro", "Acordos de Renegociação", "Prestação de Contas", "Operações", "Portaria", "Configurações" })]
    [InlineData(SmartCondoRoles.Portaria, new[] { "Início", "Portaria", "Configurações" })]
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
        grouped.Should().Contain(i => i.Label == "Início");
    }

    [Theory]
    [InlineData(SmartCondoRoles.Condomino, "Unidades e Moradores")]
    [InlineData(SmartCondoRoles.Condomino, "WhatsApp / IA")]
    [InlineData(SmartCondoRoles.Portaria, "Financeiro")]
    [InlineData(SmartCondoRoles.Portaria, "Operações")]
    public void GetItemsForRole_Should_ExcludeUnauthorizedItems(string role, string forbiddenLabel)
    {
        NavMenuItems.GetItemsForRole(role).Select(i => i.Label).Should().NotContain(forbiddenLabel);
    }
}
