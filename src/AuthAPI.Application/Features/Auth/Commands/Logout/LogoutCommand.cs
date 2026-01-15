using AuthAPI.Domain.Common.Results;
using FluentValidation;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Commands.Logout;

public record LogoutCommand() : ICommand<Result>;
public class LogoutCommandValidator : AbstractValidator<LogoutCommand> { }
