using AuthAPI.Application.Common.Interfaces;
using AuthAPI.Application.Common.Interfaces.Repositories;
using AuthAPI.Domain.Common.Results;
using MapsterMapper;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    IUserContext userContext,
    IMapper mapper
) : IQueryHandler<GetCurrentUserQuery, Result<UserResult>>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUserContext _userContext = userContext;
    private readonly IMapper _mapper = mapper;

    public async ValueTask<Result<UserResult>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return Error.NotFound($"User with ID {userId} was not found");
        var userResult = _mapper.Map<UserResult>(user);
        return userResult;
    }
}
