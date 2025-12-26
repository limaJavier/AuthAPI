using AuthAPI.Api.Exceptions;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Application.Features.Auth.Commands.RegisterWithEmail;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.RegisterWithEmail;

public class RegisterWithEmailEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<RegisterWithEmailRequest, VerificationResponse>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
    }

    public override async Task<VerificationResponse> ExecuteAsync(RegisterWithEmailRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<RegisterWithEmailCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);
        var response = _mapper.Map<VerificationResponse>(result.Value);
        return response;
    }
}