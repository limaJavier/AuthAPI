using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Utils.Extensions;
using AuthAPI.Application.Features.Auth.Commands.Logout;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.Logout;

public class LogoutEndpoint(ISender sender) : EndpointWithoutRequest
{
    private readonly ISender _sender = sender;

    public override void Configure()
    {
        Post("/auth/logout");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var command = new LogoutCommand();
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);

        HttpContext.RemoveRefreshToken(); // Remove refresh token cookie

        await Send.OkAsync();
    }
}
