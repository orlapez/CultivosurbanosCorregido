using System.Text;
using System.Text.Json;

namespace Terpy.WEB.Repositories;

public class Repository : IRepository
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Repository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseWrapper<T>> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        return await ReadResponseAsync<T>(response);
    }

    public async Task<HttpResponseWrapper<object>> PostAsync<T>(string url, T model)
    {
        using var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        return new HttpResponseWrapper<object>(null, !response.IsSuccessStatusCode, response);
    }

    public async Task<HttpResponseWrapper<TResponse>> PostAsync<T, TResponse>(string url, T model)
    {
        using var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        return await ReadResponseAsync<TResponse>(response);
    }

    public async Task<HttpResponseWrapper<object>> PutAsync<T>(string url, T model)
    {
        using var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(url, content);
        return new HttpResponseWrapper<object>(null, !response.IsSuccessStatusCode, response);
    }

    public async Task<HttpResponseWrapper<TResponse>> PutAsync<T, TResponse>(string url, T model)
    {
        using var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync(url, content);
        return await ReadResponseAsync<TResponse>(response);
    }

    public async Task<HttpResponseWrapper<object>> DeleteAsync(string url)
    {
        var response = await _httpClient.DeleteAsync(url);
        return new HttpResponseWrapper<object>(null, !response.IsSuccessStatusCode, response);
    }

    private async Task<HttpResponseWrapper<T>> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            return new HttpResponseWrapper<T>(default, true, response);
        }

        var json = await response.Content.ReadAsStringAsync();
        var value = string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, _jsonOptions);
        return new HttpResponseWrapper<T>(value, false, response);
    }
}
