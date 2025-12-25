using AuthAPI.Api.Features.Items.Responses;
using AuthAPI.Application.Features.Items.Queries.Common;
using Mapster;

namespace AuthAPI.Api.Utils.Mappings;

public class ItemMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ItemResult, ItemResponse>();
    }
}
