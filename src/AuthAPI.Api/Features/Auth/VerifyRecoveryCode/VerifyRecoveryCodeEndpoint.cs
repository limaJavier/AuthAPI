using AuthAPI.Api.Exceptions;
using AuthAPI.Application.Features.Auth.Commands.VerifyRecoveryCode;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.VerifyRecoveryCode;

public class VerifyRecoveryCodeEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<VerifyRecoveryCodeRequest>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/verify-recovery-code");
        AllowAnonymous();
    }

    public override async Task HandleAsync(VerifyRecoveryCodeRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<VerifyRecoveryCodeCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);
        await Send.OkAsync();
    }
}