using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace SmartCondo.Web.Services;

public sealed class AuthSession(ProtectedSessionStorage storage)
{
    private const string StorageKey = "smartcondo.auth";
    private bool _loaded;

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public List<AuthProfileModel> Profiles { get; set; } = [];
    public AuthContextModel? Context { get; set; }

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        var result = await storage.GetAsync<AuthSnapshot>(StorageKey);
        if (result.Success && result.Value is not null)
        {
            AccessToken = result.Value.AccessToken;
            RefreshToken = result.Value.RefreshToken;
            Profiles = result.Value.Profiles ?? [];
            Context = result.Value.Context;
        }

        _loaded = true;
    }

    public async Task PersistAsync()
    {
        await storage.SetAsync(StorageKey, ToSnapshot());
        _loaded = true;
    }

    public async Task ClearAsync()
    {
        await storage.DeleteAsync(StorageKey);
        AccessToken = null;
        RefreshToken = null;
        Profiles = [];
        Context = null;
        _loaded = true;
    }

    private AuthSnapshot ToSnapshot() => new()
    {
        AccessToken = AccessToken,
        RefreshToken = RefreshToken,
        Profiles = Profiles,
        Context = Context
    };

    private sealed class AuthSnapshot
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public List<AuthProfileModel>? Profiles { get; set; }
        public AuthContextModel? Context { get; set; }
    }
}
