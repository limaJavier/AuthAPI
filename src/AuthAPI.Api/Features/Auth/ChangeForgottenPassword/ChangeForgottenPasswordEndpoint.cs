using AuthAPI.Api.Exceptions;
using AuthAPI.Application.Features.Auth.Commands.ChangeForgottenPassword;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.ChangeForgottenPassword;

public class ChangeForgottenPasswordEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<ChangeForgottenPasswordRequest>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/change-forgotten-password");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ChangeForgottenPasswordRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<ChangeForgottenPasswordCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);
        await Send.OkAsync();
    }
}