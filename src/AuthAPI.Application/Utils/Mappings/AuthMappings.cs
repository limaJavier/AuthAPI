using AuthAPI.Application.Features.Auth.Queries.GetCurrentUser;
using AuthAPI.Domain.UserAggregate;
using Mapster;

namespace AuthAPI.Application.Utils.Mappings;

public class AuthMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserResult>();
    }
}
