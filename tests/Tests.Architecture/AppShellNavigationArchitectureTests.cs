using FluentAssertions;

namespace Tests.Architecture;

public class AppShellNavigationArchitectureTests
{
    private static string ReadWebFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, "src", "Web", "SmartCondo.Web", relativePath);
        return File.ReadAllText(fullPath);
    }

    [Fact]
    public void AppShellLayout_Should_ComposeInteractiveIslandsWithoutRenderMode()
    {
        var content = ReadWebFile("Components/Layout/AppShellLayout.razor");

        content.Should().Contain("<AppNavigation");
        content.Should().Contain("<TenantSwitcher");
        content.Should().Contain("<UserMenu");
        content.Should().Contain("app-topbar");
        content.Should().NotContain("@rendermode");
    }

    [Fact]
    public void AppNavigation_Should_OwnSidebarAndInteractiveRenderMode()
    {
        var content = ReadWebFile("Components/Layout/AppNavigation.razor");

        content.Should().Contain("@rendermode");
        content.Should().Contain("InteractiveServerRenderMode");
        content.Should().Contain("prerender: false");
        content.Should().Contain("app-sidebar");
        content.Should().Contain("<NavMenu");
        content.Should().Contain("nav-toggle");
    }

    [Fact]
    public void NavMenu_Should_UseNavLinkWithAriaCurrent()
    {
        var content = ReadWebFile("Components/Layout/NavMenu.razor");

        content.Should().Contain("NavLink");
        content.Should().Contain("aria-current");
        content.Should().Contain("NavMenuItems");
    }

    [Fact]
    public void App_Should_IncludeNavStylesheet()
    {
        var content = ReadWebFile("Components/App.razor");
        content.Should().Contain("css/stitch/nav.css");
    }

    [Fact]
    public void UserMenu_Should_BeAnInteractiveIsland()
    {
        var content = ReadWebFile("Components/Layout/UserMenu.razor");

        content.Should().Contain("@rendermode");
        content.Should().Contain("InteractiveServerRenderMode");
        content.Should().Contain("prerender: false");
    }

    private static string FindRepoRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory, "SmartCondo.slnx")))
            {
                return directory;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }
}
