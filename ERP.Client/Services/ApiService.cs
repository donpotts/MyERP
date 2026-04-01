using System.Net.Http.Json;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using ERP.Shared.DTOs;

namespace ERP.Client.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;

    public ApiService(HttpClient http, ILocalStorageService localStorage)
    {
        _http = http;
        _localStorage = localStorage;
    }

    private async Task EnsureAuthHeaderAsync()
    {
        if (_http.DefaultRequestHeaders.Authorization is null)
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");
            if (!string.IsNullOrWhiteSpace(token))
            {
                token = token.Trim('"');
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        await EnsureAuthHeaderAsync();
        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch { return default; }
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        await EnsureAuthHeaderAsync();
        try
        {
            var response = await _http.PostAsJsonAsync(url, data);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch { return default; }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
    {
        await EnsureAuthHeaderAsync();
        try
        {
            var response = await _http.PutAsJsonAsync(url, data);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
        catch { return default; }
    }

    public async Task<ApiResponse<string>?> DeleteAsync(string url)
    {
        await EnsureAuthHeaderAsync();
        try
        {
            var response = await _http.DeleteAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        }
        catch { return default; }
    }

    public async Task<ApiResponse<string>?> PostFileAsync(string url, MultipartFormDataContent content)
    {
        await EnsureAuthHeaderAsync();
        try
        {
            var response = await _http.PostAsync(url, content);
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        }
        catch { return default; }
    }
}
