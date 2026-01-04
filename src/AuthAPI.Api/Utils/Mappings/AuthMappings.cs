using AuthAPI.Api.Features.Auth.ChangeForgottenPassword;
using AuthAPI.Api.Features.Auth.ChangePassword;
using AuthAPI.Api.Features.Auth.Common.Responses;
using AuthAPI.Api.Features.Auth.ForgotPassword;
using AuthAPI.Api.Features.Auth.LoginWithEmail;
using AuthAPI.Api.Features.Auth.RegisterWithEmail;
using AuthAPI.Api.Features.Auth.VerifyEmail;
using AuthAPI.Api.Features.Auth.VerifyRecoveryCode;
using AuthAPI.Application.Features.Auth.Commands.ChangeForgottenPassword;
using AuthAPI.Application.Features.Auth.Commands.ChangePassword;
using AuthAPI.Application.Features.Auth.Commands.Common;
using AuthAPI.Application.Features.Auth.Commands.ForgotPassword;
using AuthAPI.Application.Features.Auth.Commands.LoginWithEmail;
using AuthAPI.Application.Features.Auth.Commands.RegisterWithEmail;
using AuthAPI.Application.Features.Auth.Commands.VerifyEmail;
using Mapster;

namespace AuthAPI.Api.Utils.Mappings;

public class AuthMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        //** Requests mappings
        config.NewConfig<RegisterWithEmailRequest, RegisterWithEmailCommand>();
        config.NewConfig<VerifyEmailRequest, VerifyEmailCommand>();
        config.NewConfig<LoginWithEmailRequest, LoginWithEmailCommand>();
        config.NewConfig<ForgotPasswordRequest, ForgotPasswordCommand>();
        config.NewConfig<VerifyRecoveryCodeRequest, VerifyRecoveryCodeRequest>();
        config.NewConfig<ChangeForgottenPasswordRequest, ChangeForgottenPasswordCommand>();
        config.NewConfig<ChangePasswordRequest, ChangePasswordCommand>();

        //** Responses mappings
        config.NewConfig<VerificationResult, VerificationResponse>();
        config.NewConfig<AuthResult, AuthResponse>()
            .Map(dest => dest.AccessToken, src => src.AccessToken);
    }
}
