using AuthAPI.Api.Exceptions;
using AuthAPI.Application.Features.Auth.Commands.ChangePassword;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.ChangePassword;

public class ChangePasswordEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<ChangePasswordRequest>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/change-password");
    }

    public override async Task HandleAsync(ChangePasswordRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<ChangePasswordCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);
        await Send.OkAsync();
    }
}