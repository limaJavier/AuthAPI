using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.RegisterWithEmail;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Application.Features.Auth.Commands.RegisterWithEmail;
using Mapster;

namespace AuthAPI.Api.Utils.Mappings;

public class AuthMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //** Requests mappings
        config.NewConfig<RegisterWithEmailRequest, RegisterWithEmailCommand>();


        //** Responses mappings
        config.NewConfig<VerificationResult, VerificationResponse>();
    }
}
