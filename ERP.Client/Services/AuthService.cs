using System.Net.Http.Json;
using Blazored.LocalStorage;
using ERP.Shared.DTOs;

namespace ERP.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtAuthStateProvider _authState;

    public AuthService(HttpClient http, ILocalStorageService localStorage, Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider authState)
    {
        _http = http;
        _localStorage = localStorage;
        _authState = (JwtAuthStateProvider)authState;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Message = "Failed" };

        if (result.IsSuccess && result.Token is not null)
        {
            await _localStorage.SetItemAsStringAsync("authToken", result.Token);
            if (result.User is not null)
                await _localStorage.SetItemAsync("userInfo", result.User);
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            _authState.NotifyAuthStateChanged();
        }
        return result;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse { Message = "Failed" };

        if (result.IsSuccess && result.Token is not null)
        {
            await _localStorage.SetItemAsStringAsync("authToken", result.Token);
            if (result.User is not null)
                await _localStorage.SetItemAsync("userInfo", result.User);
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);
            _authState.NotifyAuthStateChanged();
        }
        return result;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("userInfo");
        _http.DefaultRequestHeaders.Authorization = null;
        _authState.NotifyAuthStateChanged();
    }

    public event Action? OnUserChanged;

    public async Task<UserInfo?> GetUserInfoAsync()
    {
        return await _localStorage.GetItemAsync<UserInfo>("userInfo");
    }

    public async Task UpdateAvatarAsync(string avatarUrl)
    {
        var user = await GetUserInfoAsync();
        if (user is not null)
        {
            user.AvatarUrl = avatarUrl;
            await _localStorage.SetItemAsync("userInfo", user);
            OnUserChanged?.Invoke();
        }
    }

    public async Task InitializeAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            token = token.Trim('"');
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
