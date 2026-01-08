using System.Text.Json;
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

        var errors = new Dictionary<string, List<string>>();
        foreach (var error in validationResult.Errors)
        {
            if (errors.ContainsKey(error.PropertyName))
                errors[error.PropertyName].Add(error.ErrorMessage);
            else
                errors.Add(error.PropertyName, [error.ErrorMessage]);
        }
        var errorsJson = JsonSerializer.Serialize(errors);

        return (dynamic)Error.Validation("A validation error occurred", errorsJson);
    }
}
