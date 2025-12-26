using System.Net.Http.Json;

namespace AuthAPI.Api.Tests.Features.Utils;

public static class HttpClientExtensions
{
    public static async Task<T> SendAsync<T>(
        this HttpClient client,
        HttpMethod method,
        string route,
        object? body = null,
        string? accessToken = null,
        string? refreshToken = null
    )
    {
        using var httpRequest = new HttpRequestMessage(method, route);

        if (accessToken is not null)
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

        if (refreshToken is not null)
            httpRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");

        if (body is not null)
            httpRequest.Content = JsonContent.Create(body);

        var httpResponse = await client.SendAsync(httpRequest);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<T>()
            ?? throw new Exception("Cannot read response body");

        return response;
    }

    public static async Task<HttpResponseMessage> SendAsync(
        this HttpClient client,
        HttpMethod method,
        string route,
        object? body = null,
        string? accessToken = null,
        string? refreshToken = null
    )
    {
        using var httpRequest = new HttpRequestMessage(method, route);

        if (accessToken is not null)
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

        if (refreshToken is not null)
            httpRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");

        if (body is not null)
            httpRequest.Content = JsonContent.Create(body);

        var httpResponse = await client.SendAsync(httpRequest);

        return httpResponse;
    }

    public static async Task<HttpResponseMessage> SendAndEnsureSuccessAsync(
        this HttpClient client,
        HttpMethod method,
        string route,
        object? body = null,
        string? accessToken = null,
        string? refreshToken = null
    )
    {
        using var httpRequest = new HttpRequestMessage(method, route);

        if (accessToken is not null)
            httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

        if (refreshToken is not null)
            httpRequest.Headers.Add("Cookie", $"refresh_token={refreshToken}");

        if (body is not null)
            httpRequest.Content = JsonContent.Create(body);

        var httpResponse = await client.SendAsync(httpRequest);
        httpResponse.EnsureSuccessStatusCode();

        return httpResponse;
    }
}