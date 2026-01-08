using System.Net;
using AuthAPI.Api.Utils.Extensions;
using AuthAPI.Domain.Common.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace AuthAPI.Api.Exceptions;

public static class GlobalExceptionHandler
{
    private const string DefaultTitle = "An unexpected error occurred in the server";

    public static async Task Handle(HttpContext httpContext)
    {
        var exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();

        if (exceptionHandlerFeature is not null)
        {
            var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(nameof(GlobalExceptionHandler)); // Create static context logger

            var exception = exceptionHandlerFeature.Error;
            var traceId = httpContext.GetTraceId(); // Resolve trace-id
            var userId = httpContext.GetUserId(); // Resolve logged user's id

            // Log error
            logger.LogError(
                "An error occurred while processing the request {Method} {Path} TraceId={TraceId} from UserId={UserId}: {ErrorMessage}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId,
                userId,
                exception.Message
            );

            IResult response;
            if (exception is ApiException apiException)
            {
                HttpStatusCode statusCode = apiException.Type switch
                {
                    ErrorType.Unexpected => HttpStatusCode.InternalServerError,
                    ErrorType.Validation => HttpStatusCode.BadRequest,
                    ErrorType.Conflict => HttpStatusCode.Conflict,
                    ErrorType.NotFound => HttpStatusCode.NotFound,
                    ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
                    ErrorType.Forbidden => HttpStatusCode.Forbidden,
                    _ => HttpStatusCode.InternalServerError
                };

                response = apiException.Message == string.Empty ?
                    Results.Problem(statusCode: (int)statusCode) :
                    apiException.Detail is null ?
                        Results.Problem(
                            title: apiException.Message,
                            statusCode: (int)statusCode) :
                        Results.Problem(
                            title: apiException.Message,
                            statusCode: (int)statusCode,
                            detail: apiException.Detail);
            }
            else
            {
                response = Results.Problem(
                    title: DefaultTitle,
                    detail: exception.Message,
                    statusCode: (int)HttpStatusCode.InternalServerError
                );
            }

            // Write response
            await response.ExecuteAsync(httpContext);
        }
    }
}
