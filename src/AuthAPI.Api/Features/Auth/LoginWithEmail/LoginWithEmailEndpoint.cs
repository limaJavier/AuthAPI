using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Utils.Extensions;
using AuthAPI.Application.Features.Auth.Commands.LoginWithEmail;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.LoginWithEmail;

public class LoginWithEmailEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<LoginWithEmailRequest, AuthResponse>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
    }

    public override async Task<AuthResponse> ExecuteAsync(LoginWithEmailRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<LoginWithEmailCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);

        HttpContext.AddRefreshToken(result.Value.RefreshToken);
        var response = _mapper.Map<AuthResponse>(result.Value);
        return response;
    }
}