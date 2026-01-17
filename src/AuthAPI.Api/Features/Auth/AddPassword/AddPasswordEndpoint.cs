using AuthAPI.Api.Exceptions;
using AuthAPI.Application.Features.Auth.Commands.AddPassword;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.AddPassword;

public class AddPasswordEndpoint(
    ISender sender,
    IMapper mapper
) : Endpoint<AddPasswordRequest>
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Post("/auth/add-password");
    }

    public override async Task HandleAsync(AddPasswordRequest request, CancellationToken ct)
    {
        var command = _mapper.Map<AddPasswordCommand>(request);
        var result = await _sender.Send(command);
        if (result.IsFailure)
            throw ApiException.FromError(result.Error);
        await Send.OkAsync();
    }
}
