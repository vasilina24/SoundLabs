using Microsoft.JSInterop;
using SoundLabs.Blazor.Models;
using System.Text.Json;
namespace SoundLabs.Blazor.Services;
public sealed class AuthStore(IJSRuntime js)
{
    public AuthResponse? Current { get; private set; }
    public event Action? Changed;
    public bool IsLoggedIn => Current is not null;
    public string Role => Current?.Role ?? "";
    public async Task InitializeAsync()
    {
        try { var raw = await js.InvokeAsync<string?>("localStorage.getItem", "soundlabs-auth"); Current = string.IsNullOrWhiteSpace(raw) ? null : JsonSerializer.Deserialize<AuthResponse>(raw); }
        catch { Current = null; }
        Changed?.Invoke();
    }
    public async Task SetAsync(AuthResponse auth) { Current = auth; await js.InvokeVoidAsync("localStorage.setItem", "soundlabs-auth", JsonSerializer.Serialize(auth)); Changed?.Invoke(); }
    public async Task LogoutAsync() { Current = null; await js.InvokeVoidAsync("localStorage.removeItem", "soundlabs-auth"); Changed?.Invoke(); }
}
