using System.Security.Claims;
using AuthAPI.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AuthAPI.Infrastructure.Services;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    public Guid UserId => GetUserId();
    public Guid SessionId => GetSessionId();

    private Guid GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new Exception("Cannot resolve the HttpContext");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Cannot resolve the name-identifier claim");

        return Guid.Parse(userIdClaim.Value);
    }

    private Guid GetSessionId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new Exception("Cannot resolve the HttpContext");

        var sessionId = httpContext.User.FindFirst(ClaimTypes.Sid)
            ?? throw new Exception("Cannot resolve the session-id claim");

        return Guid.Parse(sessionId.Value);
    }
}
