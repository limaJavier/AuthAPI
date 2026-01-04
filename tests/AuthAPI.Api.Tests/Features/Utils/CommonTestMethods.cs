using System.Net;
namespace AuthAPI.Api.Tests.Features.Utils;

public static class CommonTestMethods
{
    public static async Task WhenAccessTokenIsMissing_ShouldReturnUnauthorized(
        HttpClient client,
        HttpMethod httpMethod,
        string route
    )
    {
        //** Act
        var httpResponse = await client.SendAsync(
            method: httpMethod,
            route: route
        );

        //** Assert
        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
    }

    public static async Task WhenRequestIsBad_ShouldReturnBadRequest(
        HttpClient client,
        HttpMethod httpMethod,
        string route,
        object request,
        string? accessToken = null
    )
    {
        //** Act
        var httpResponse = await client.SendAsync(
            method: httpMethod,
            route: route,
            body: request,
            accessToken: accessToken
        );

        //** Assert
        Assert.Equal(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }
}