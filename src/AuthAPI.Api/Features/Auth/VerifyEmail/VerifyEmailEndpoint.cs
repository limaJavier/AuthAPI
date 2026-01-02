using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Utils.Extensions;
using AuthAPI.Application.Features.Auth.Commands.VerifyEmail;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.VerifyEmail;

public class VerifyEmailEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<VerifyEmailRequest, AuthResponse>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/verify-email");
        AllowAnonymous();
    }

    public override async Task<AuthResponse> ExecuteAsync(VerifyEmailRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<VerifyEmailCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);

        HttpContext.AddRefreshToken(result.Value.RefreshToken);
        var response = _mapper.Map<AuthResponse>(result.Value);
        return response;
    }
}