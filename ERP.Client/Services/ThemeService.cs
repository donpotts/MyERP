using Microsoft.JSInterop;

namespace ERP.Client.Services;

public class ThemeService
{
    private readonly IJSRuntime _js;

    public string Theme  { get; private set; } = "light";
    public string Accent { get; private set; } = "indigo";
    public bool   IsDark => Theme == "dark";

    public event Action? OnChange;

    public ThemeService(IJSRuntime js) => _js = js;

    public async Task InitAsync()
    {
        Theme  = await _js.InvokeAsync<string>("themeManager.getTheme");
        Accent = await _js.InvokeAsync<string>("themeManager.getAccent");
    }

    public async Task ToggleThemeAsync()
    {
        Theme = IsDark ? "light" : "dark";
        await _js.InvokeVoidAsync("themeManager.setTheme", Theme);
        OnChange?.Invoke();
    }

    public async Task SetAccentAsync(string accent)
    {
        Accent = accent;
        await _js.InvokeVoidAsync("themeManager.setAccent", accent);
        OnChange?.Invoke();
    }

    public string TopBarClass() => Accent switch
    {
        "violet"  => "bg-gradient-to-r from-violet-600 to-purple-700",
        "blue"    => "bg-gradient-to-r from-blue-600 to-blue-700",
        "emerald" => "bg-gradient-to-r from-emerald-600 to-teal-700",
        "rose"    => "bg-gradient-to-r from-rose-600 to-pink-700",
        "amber"   => "bg-gradient-to-r from-amber-500 to-orange-600",
        _         => "bg-gradient-to-r from-indigo-600 to-purple-600",
    };

    public string AccentTextClass() => Accent switch
    {
        "violet"  => "text-violet-500",
        "blue"    => "text-blue-500",
        "emerald" => "text-emerald-500",
        "rose"    => "text-rose-500",
        "amber"   => "text-amber-500",
        _         => "text-indigo-500",
    };
}
