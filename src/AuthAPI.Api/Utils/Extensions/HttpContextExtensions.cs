using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace AuthAPI.Api.Utils.Extensions;

public static class HttpContextExtensions
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public static string GetTraceId(this HttpContext httpContext) => Activity.Current?.Id ?? httpContext.TraceIdentifier;

    public static string? GetAccessToken(this HttpContext httpContext) => httpContext.GetTokenAsync(AccessTokenKey).Result;

    public static string? GetRefreshToken(this HttpContext httpContext) => httpContext.Request.Cookies[RefreshTokenKey];

    public static Guid? GetUserId(this HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim is null ? null : Guid.Parse(userIdClaim.Value);
    }

    public static void AddRefreshToken(this HttpContext httpContext, string refreshToken)
    {
        // TODO: Avoid hardcode expiry date
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMonths(1)
        };

        httpContext.Response.Cookies.Append(RefreshTokenKey, refreshToken, options);
    }
}
