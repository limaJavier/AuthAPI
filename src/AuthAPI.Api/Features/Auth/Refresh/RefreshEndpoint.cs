using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Utils.Extensions;
using AuthAPI.Application.Features.Auth.Commands.Refresh;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.Refresh;

public class RefreshEndpoint(
    ISender sender,
    IMapper mapper
) : EndpointWithoutRequest<AuthResponse>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
    }

    public override async Task<AuthResponse> ExecuteAsync(CancellationToken ct)
    {
        var refreshToken = HttpContext.GetRefreshToken() 
            ?? throw ApiException.Conflict("Cannot resolve refresh_token cookie");

        var command = new RefreshCommand(refreshToken);
        var result = await _sender.Send(command);
        if(result.IsFailure)
            throw ApiException.FromError(result.Error);

        HttpContext.AddRefreshToken(result.Value.RefreshToken);
        var response = _mapper.Map<AuthResponse>(result.Value);
        return response;
    }
}