using Microsoft.AspNetCore.Mvc;
using Common;
using Inventory.Service.Dtos;
using Inventory.Service.Dtos.Entities;

namespace Inventory.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _inventoryItemRepository;

    private readonly IRepository<CatalogItem> _catalogItemRepository;

    public ItemsController(IRepository<InventoryItem> inventoryItemRepository, IRepository<CatalogItem> catalogItemRepository)
    {
        this._inventoryItemRepository = inventoryItemRepository;
        this._catalogItemRepository = catalogItemRepository;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var inventoryItemEntities = await _inventoryItemRepository.GetAllAsync(item => item.UserId == userId);
        var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);
        var catalogItemEntities = await _catalogItemRepository.GetAllAsync(item => itemIds.Contains(item.Id));
        var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });
        
        return Ok(inventoryItemDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto model)
    {
        var inventoryItem = await _inventoryItemRepository.GetAsync(item =>
            item.UserId == model.UserId && item.CatalogItemId == model.CatalogItemId);

        if (inventoryItem == null)
        {
            inventoryItem = new InventoryItem
            {
                CatalogItemId = model.CatalogItemId,
                UserId = model.UserId,
                Quantity = model.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await _inventoryItemRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += model.Quantity;
            await _inventoryItemRepository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }
}