using Inventory.Service.Dtos;

namespace Inventory.Service.Clients;

public class CatalogClient
{
    private readonly HttpClient _httpClient;

    public CatalogClient(HttpClient httpClient)
    {
        this._httpClient = httpClient; 
    }

    public async Task<IReadOnlyCollection<CatalogItemDto>> GetCatalogItemsAsync()
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<CatalogItemDto>>("items");
    }
}