using AuthAPI.Application.Features.Items.Queries.Common;
using AuthAPI.Domain.Common.Results;
using Mediator;

namespace AuthAPI.Application.Features.Items.Queries.GetItems;

public record GetItemsQuery() : IQuery<Result<List<ItemResult>>>;
