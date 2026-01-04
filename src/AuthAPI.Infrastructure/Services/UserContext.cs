using System.Security.Claims;
using AuthAPI.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AuthAPI.Infrastructure.Services;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    public Guid UserId => GetUserId();

    private Guid GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new Exception("Cannot resolve the HttpContext");

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Cannot resolve the name-identifier claim");

        return Guid.Parse(userIdClaim.Value);
    }
}
