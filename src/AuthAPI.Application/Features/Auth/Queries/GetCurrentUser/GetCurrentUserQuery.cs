using AuthAPI.Domain.Common.Results;
using FluentValidation;
using Mediator;

namespace AuthAPI.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery() : IQuery<Result<UserResult>>;
public class GetCurrentUserQueryValidator : AbstractValidator<GetCurrentUserQuery> { }
