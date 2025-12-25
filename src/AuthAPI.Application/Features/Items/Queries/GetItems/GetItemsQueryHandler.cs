using AuthAPI.Application.Features.Items.Queries.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Items.Queries.GetItems;

public class GetItemsQueryHandler : IQueryHandler<GetItemsQuery, Result<List<ItemResult>>>
{
    public async ValueTask<Result<List<ItemResult>>> Handle(GetItemsQuery command, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var items = new List<ItemResult>
        {
            new("Item 1", "Description 1"),
            new("Item 2", "Description 2"),
            new("Item 3", "Description 3"),
            new("Item 4", "Description 4"),
        };

        return items;
    }
}
