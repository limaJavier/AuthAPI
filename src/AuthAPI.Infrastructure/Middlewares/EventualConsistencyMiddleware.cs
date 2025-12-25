using AuthAPI.Application.Common;
using AuthAPI.Domain.Common;
using AuthAPI.Infrastructure.Persistence;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AuthAPI.Infrastructure.Middlewares;

public class EventualConsistencyMiddleware(RequestDelegate next)
{
    public const string DomainEventsKey = "DomainEventsKey";
    public const string ApplicationEventsKey = "ApplicationEventsKey";
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IPublisher publisher,
        AuthAPIDbContext dbContext,
        ILogger<EventualConsistencyMiddleware> logger)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync();
        context.Response.OnCompleted(async () =>
        {
            try
            {
                // Process domain events
                if (context.Items.TryGetValue(DomainEventsKey, out var domainValue) && domainValue is Queue<IDomainEvent> domainEvents)
                {
                    while (domainEvents.TryDequeue(out var nextEvent))
                    {
                        await publisher.Publish(nextEvent);
                    }
                }

                await transaction.CommitAsync();

                // Process application events
                if (context.Items.TryGetValue(ApplicationEventsKey, out var applicationValue) && applicationValue is Queue<IApplicationEvent> applicationEvents)
                {
                    while (applicationEvents.TryDequeue(out var nextEvent))
                    {
                        await publisher.Publish(nextEvent);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError("An error occurred while processing an event: {ErrorMessage}", e.Message);
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        });

        await _next(context);
    }
}
