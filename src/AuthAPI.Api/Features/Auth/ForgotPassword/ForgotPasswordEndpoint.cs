using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Application.Features.Auth.Commands.ForgotPassword;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.ForgotPassword;

public class ForgotPasswordEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<ForgotPasswordRequest, VerificationResponse>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/forgot-password");
        AllowAnonymous();
    }

    public override async Task<VerificationResponse> ExecuteAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<ForgotPasswordCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);
        var response = _mapper.Map<VerificationResponse>(result.Value);
        return response;
    }
}