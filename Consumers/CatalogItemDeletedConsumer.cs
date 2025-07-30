using Catalog.Contracts;
using Common.Library;
using Trading.Service.Entities;
using MassTransit;

namespace Trading.Service.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> repository;
    public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
    {
        this.repository = repository;
    }
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;
        var item = await repository.GetAsync(message.ItemId);
        if (item == null)
        {
            return;
        }
        await repository.RemoveAsync(message.ItemId);
    }
}
