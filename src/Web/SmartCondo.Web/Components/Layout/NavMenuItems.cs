using Modules.Identity.Domain;

namespace SmartCondo.Web.Components.Layout;

public enum NavSection
{
    Principal,
    Gestao,
    Acesso,
    Inteligencia
}

public sealed record NavMenuItem(
    string Label,
    string Href,
    string Icon,
    NavSection Section,
    IReadOnlyList<string> AllowedRoles,
    bool IsFooter = false);

public static class NavMenuItems
{
    public static IReadOnlyList<NavMenuItem> All { get; } =
    [
        new("Início", "/", "dashboard", NavSection.Principal,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino, SmartCondoRoles.Portaria, SmartCondoRoles.Zelador]),
        new("Unidades e Moradores", "/unidades", "apartment", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora]),
        new("Financeiro", "/financeiro", "payments", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Operações", "/operacoes", "build", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Portaria", "/portaria", "badge", NavSection.Acesso,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino, SmartCondoRoles.Portaria]),
        new("WhatsApp / IA", "/whatsapp", "smart_toy", NavSection.Inteligencia,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora]),
        new("Configurações", "/configuracoes", "settings", NavSection.Principal,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino, SmartCondoRoles.Portaria, SmartCondoRoles.Zelador],
            IsFooter: true)
    ];

    public static IEnumerable<NavMenuItem> GetItemsForRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return [];
        }

        return All.Where(item => item.AllowedRoles.Contains(role));
    }

    public static IEnumerable<NavMenuItem> GetMainItemsForRole(string? role) =>
        GetItemsForRole(role).Where(item => !item.IsFooter);

    public static NavMenuItem? GetFooterItemForRole(string? role) =>
        GetItemsForRole(role).FirstOrDefault(item => item.IsFooter);

    public static IEnumerable<IGrouping<NavSection, NavMenuItem>> GetGroupedMainItemsForRole(string? role) =>
        GetMainItemsForRole(role).GroupBy(item => item.Section);

    public static string GetSectionLabel(NavSection section) => section switch
    {
        NavSection.Principal => "Principal",
        NavSection.Gestao => "Gestão",
        NavSection.Acesso => "Acesso",
        NavSection.Inteligencia => "Inteligência",
        _ => section.ToString()
    };
}
