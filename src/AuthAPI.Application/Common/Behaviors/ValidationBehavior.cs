using AuthAPI.Domain.Common.Results;
using FluentValidation;
using Mediator;

namespace AuthAPI.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest> validator) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
    where TResponse : ResultBase
{
    private readonly IValidator<TRequest> _validator = validator;

    public async ValueTask<TResponse> Handle(TRequest request, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.IsValid)
        {
            return await next(request, cancellationToken);
        }

        var errorMessage = string.Join(": ", validationResult.Errors.Select(error => error.ErrorMessage));

        return (dynamic)Error.Validation(errorMessage);
    }
}
