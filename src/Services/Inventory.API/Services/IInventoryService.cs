using Inventory.API.DTOs;

namespace Inventory.API.Services
{
    public interface IInventoryService
    {
        // Operações básicas de CRUD
        Task<IEnumerable<InventoryItemDto>> GetAllInventoryItemsAsync();
        Task<InventoryItemDto?> GetInventoryItemByIdAsync(Guid id);
        Task<InventoryItemDto?> GetInventoryItemByProductIdAsync(Guid productId);
        Task<InventoryItemDto?> GetInventoryItemBySkuAsync(string sku);
        Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemDto createInventoryItemDto);
        Task<InventoryItemDto?> UpdateInventoryItemAsync(Guid id, UpdateInventoryItemDto updateInventoryItemDto);
        Task<bool> DeleteInventoryItemAsync(Guid id);

        // Operações de movimentação de estoque
        Task<bool> AddStockAsync(Guid productId, int quantity, string reason = "");
        Task<bool> RemoveStockAsync(Guid productId, int quantity, string reason = "");
        Task<bool> ReserveStockAsync(Guid productId, int quantity, string reason = "");
        Task<bool> ReleaseReservedStockAsync(Guid productId, int quantity, string reason = "");

        // Consultas de estoque
        Task<StockCheckDto?> CheckStockAsync(Guid productId);
        Task<StockCheckDto?> CheckStockBySkuAsync(string sku);
        Task<bool> IsStockAvailableAsync(Guid productId, int requiredQuantity);
        Task<IEnumerable<InventoryItemDto>> GetLowStockItemsAsync();
        Task<IEnumerable<InventoryItemDto>> GetOutOfStockItemsAsync();
        Task<IEnumerable<InventoryItemDto>> GetInventoryByLocationAsync(string location);
    }
} 