using MassTransit;
using Catalog.Contracts;
using Common;
using Inventory.Service.Dtos.Entities;

namespace Inventory.Service.Consumers;

public class CatalogItemCreatedConsumer: IConsumer<CatalogItemCreated>
{
    private readonly IRepository<CatalogItem> _repository;

    public CatalogItemCreatedConsumer(IRepository<CatalogItem> repository)
    {
        this._repository = repository;
    }

    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        var message = context.Message;

        var item = await _repository.GetAsync(message.ItemId);

        if (item != null)
        {
            return;
        }

        item = new CatalogItem
        {
            Id = message.ItemId,
            Name = message.Name,
            Description = message.Description
        };

        await _repository.CreateAsync(item);
    }
}