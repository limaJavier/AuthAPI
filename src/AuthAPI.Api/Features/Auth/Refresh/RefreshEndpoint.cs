using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Utils.Extensions;
using AuthAPI.Application.Features.Auth.Commands.Refresh;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AuthAPI.Api.Features.Auth.Refresh;

public class RefreshEndpoint(
    ISender sender,
    IMapper mapper
) : EndpointWithoutRequest<Results<Ok<AuthResponse>, UnauthorizedHttpResult>>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> ExecuteAsync(CancellationToken ct)
    {
        var refreshToken = HttpContext.GetRefreshToken()
            ?? throw ApiException.Unauthorized("Cannot resolve refresh_token cookie");

        var command = new RefreshCommand(refreshToken);
        var result = await _sender.Send(command);

        // Remove the refresh-token cookie if there was an error and it was of unauthorized type
        if (result.IsFailure && result.Error.Type == Domain.Common.Results.ErrorType.Unauthorized)
        {
            HttpContext.RemoveRefreshToken();
            return TypedResults.Unauthorized();
        }
        else if (result.IsFailure) // Handle error by default mechanism if it was not of unauthorized ype
        {
            throw ApiException.FromError(result.Error);
        }

        HttpContext.AddRefreshToken(result.Value.RefreshToken); // Add new refresh-token cookie
        var response = _mapper.Map<AuthResponse>(result.Value);
        return TypedResults.Ok(response);
    }
}
