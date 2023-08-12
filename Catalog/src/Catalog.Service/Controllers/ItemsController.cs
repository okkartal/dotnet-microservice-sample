
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Catalog.Contracts;
using Catalog.Service.Dtos;
using Catalog.Service.Entities;
using Common;



namespace Catalog.Service.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _itemRepository;

    private readonly IPublishEndpoint _publishEndpoint;
    public ItemsController(IRepository<Item> itemRepository, IPublishEndpoint publishEndpoint)
    {
        this._itemRepository = itemRepository;
        this._publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        var items = (await _itemRepository.GetAllAsync())
        .Select(item => item.AsDto());

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await _itemRepository.GetAsync(id);

        if (item == null)
        {
            return NotFound();
        }
        return item.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync([FromBody] CreateItemDto model)
    {
        var item = new Item
        {
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await _itemRepository.CreateAsync(item);

        await _publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto model)
    {
        var existingItem = await _itemRepository.GetAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.Name = model.Name;
        existingItem.Description = model.Description;
        existingItem.Price = model.Price;

        await _itemRepository.UpdateAsync(existingItem);
        
        await _publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var existingItem = await _itemRepository.GetAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        await _itemRepository.RemoveAsync(existingItem.Id);
       
        await _publishEndpoint.Publish(new CatalogItemDeleted(id));
        
        return NoContent();
    }
}
