using ERP.Shared.DTOs;

namespace ERP.Client.Services;

public class BrandingService
{
    private readonly ApiService _api;
    private CompanyBrandingDto _branding = new();
    private bool _initialized;

    public event Action? OnChange;

    public CompanyBrandingDto Current   => _branding;
    public string CompanyName           => _branding.CompanyName;
    public string CompanyTagline        => _branding.CompanyTagline;
    public string LogoText              => _branding.LogoText;
    public string? LogoUrl              => _branding.LogoUrl;
    public string? FaviconUrl           => _branding.FaviconUrl;
    public string AppTitle              => _branding.AppTitle;
    public bool   TruncateSidebarText   => _branding.TruncateSidebarText;

    public BrandingService(ApiService api) => _api = api;

    public async Task InitAsync()
    {
        if (_initialized) return;
        try
        {
            var r = await _api.GetAsync<ApiResponse<CompanyBrandingDto>>("/api/branding");
            if (r?.Data is not null) _branding = r.Data;
        }
        catch { /* fallback to defaults */ }
        _initialized = true;
    }

    public async Task RefreshAsync()
    {
        _initialized = false;
        await InitAsync();
        OnChange?.Invoke();
    }

    public async Task SaveAsync(CompanyBrandingDto dto)
    {
        var r = await _api.PutAsync<CompanyBrandingDto, ApiResponse<CompanyBrandingDto>>("/api/branding", dto);
        if (r?.Data is not null)
        {
            _branding = r.Data;
            OnChange?.Invoke();
        }
    }
}
