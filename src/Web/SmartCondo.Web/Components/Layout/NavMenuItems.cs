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
        new("Financeiro", "/financeiro/faturas", "payments", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Simulador Financeiro", "/financeiro/simulador", "calculate", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Acordos de Renegociação", "/financeiro/acordos", "handshake", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Inadimplência e Régua", "/financeiro/inadimplencia", "warning_amber", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora]),
        new("Prestação de Contas", "/financeiro/prestacao-contas", "folder_shared", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Conciliação Bancária", "/financeiro/conciliacao", "account_balance", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora]),
        new("Relatórios Consolidados", "/financeiro/relatorios", "analytics", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora]),
        new("Áreas Comuns", "/operacoes/areas-comuns", "deck", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Reservas", "/operacoes/reservas", "event_available", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino]),
        new("Ocorrências e Chamados", "/operacoes/ocorrencias", "confirmation_number", NavSection.Gestao,
            [SmartCondoRoles.Sindico, SmartCondoRoles.Administradora, SmartCondoRoles.Condomino, SmartCondoRoles.Zelador]),
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
