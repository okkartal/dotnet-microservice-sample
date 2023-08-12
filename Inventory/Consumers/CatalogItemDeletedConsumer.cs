using MassTransit;
using Catalog.Contracts;
using Common;
using Inventory.Service.Dtos.Entities;

namespace Inventory.Service.Consumers;

public class CatalogItemDeletedConsumer: IConsumer<CatalogItemDeleted>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemDeletedConsumer(IRepository<CatalogItem> repository)
    {
        this._repository = repository;
    }

    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);

        if (item == null)
        {
            return;
        }

        await _repository.RemoveAsync(message.ItemId);
    }
}