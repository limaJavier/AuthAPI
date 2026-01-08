using AuthAPI.Api.Exceptions;
using AuthAPI.Application.Features.Auth.Queries.GetCurrentUser;
using FastEndpoints;
using Mediator;

namespace AuthAPI.Api.Features.Auth.GetCurrentUser;

public class GetCurrentUserEndpoint(
    ISender sender,
    IMapper mapper
) : EndpointWithoutRequest
{
    private readonly ISender _sender = sender;
    private readonly IMapper _mapper = mapper;

    public override void Configure()
    {
        Get("/auth/me");
    }

    public override async Task<object?> ExecuteAsync(CancellationToken ct)
    {
        var query = new GetCurrentUserQuery();
        var result = await _sender.Send(query);
        if(result.IsFailure)
            throw ApiException.FromError(result.Error);
        var response = _mapper.Map<UserResponse>(result.Value);
        return response;
    }
}
