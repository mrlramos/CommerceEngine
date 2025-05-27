using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Inventory.API.Data;
using Inventory.API.DTOs;
using Inventory.API.Models;

namespace Inventory.API.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly InventoryContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(InventoryContext context, IMapper mapper, ILogger<InventoryService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetAllInventoryItemsAsync()
        {
            try
            {
                var items = await _context.InventoryItems
                    .OrderBy(i => i.SKU)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<InventoryItemDto>>(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os itens de inventário");
                throw;
            }
        }

        public async Task<InventoryItemDto?> GetInventoryItemByIdAsync(Guid id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                return item != null ? _mapper.Map<InventoryItemDto>(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar item de inventário por ID: {ItemId}", id);
                throw;
            }
        }

        public async Task<InventoryItemDto?> GetInventoryItemByProductIdAsync(Guid productId)
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);
                
                return item != null ? _mapper.Map<InventoryItemDto>(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar item de inventário por ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<InventoryItemDto?> GetInventoryItemBySkuAsync(string sku)
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.SKU == sku);
                
                return item != null ? _mapper.Map<InventoryItemDto>(item) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar item de inventário por SKU: {SKU}", sku);
                throw;
            }
        }

        public async Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemDto createInventoryItemDto)
        {
            try
            {
                var item = _mapper.Map<InventoryItem>(createInventoryItemDto);
                item.Id = Guid.NewGuid();
                item.CreatedAt = DateTime.UtcNow;
                item.LastUpdated = DateTime.UtcNow;

                _context.InventoryItems.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item de inventário criado: {ItemId} - SKU: {SKU}", item.Id, item.SKU);
                return _mapper.Map<InventoryItemDto>(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar item de inventário");
                throw;
            }
        }

        public async Task<InventoryItemDto?> UpdateInventoryItemAsync(Guid id, UpdateInventoryItemDto updateInventoryItemDto)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return null;
                }

                _mapper.Map(updateInventoryItemDto, item);
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Item de inventário atualizado: {ItemId}", id);
                return _mapper.Map<InventoryItemDto>(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar item de inventário: {ItemId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteInventoryItemAsync(Guid id)
        {
            try
            {
                var item = await _context.InventoryItems.FindAsync(id);
                if (item == null)
                {
                    return false;
                }

                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Item de inventário removido: {ItemId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover item de inventário: {ItemId}", id);
                throw;
            }
        }

        public async Task<bool> AddStockAsync(Guid productId, int quantity, string reason = "")
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (item == null)
                {
                    _logger.LogWarning("Produto não encontrado no inventário: {ProductId}", productId);
                    return false;
                }

                item.Quantity += quantity;
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estoque adicionado - ProductId: {ProductId}, Quantidade: {Quantity}, Motivo: {Reason}", 
                    productId, quantity, reason);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar estoque - ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> RemoveStockAsync(Guid productId, int quantity, string reason = "")
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (item == null)
                {
                    _logger.LogWarning("Produto não encontrado no inventário: {ProductId}", productId);
                    return false;
                }

                if (item.AvailableQuantity < quantity)
                {
                    _logger.LogWarning("Estoque insuficiente - ProductId: {ProductId}, Disponível: {Available}, Solicitado: {Requested}", 
                        productId, item.AvailableQuantity, quantity);
                    return false;
                }

                item.Quantity -= quantity;
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estoque removido - ProductId: {ProductId}, Quantidade: {Quantity}, Motivo: {Reason}", 
                    productId, quantity, reason);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover estoque - ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> ReserveStockAsync(Guid productId, int quantity, string reason = "")
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (item == null)
                {
                    _logger.LogWarning("Produto não encontrado no inventário: {ProductId}", productId);
                    return false;
                }

                if (item.AvailableQuantity < quantity)
                {
                    _logger.LogWarning("Estoque insuficiente para reserva - ProductId: {ProductId}, Disponível: {Available}, Solicitado: {Requested}", 
                        productId, item.AvailableQuantity, quantity);
                    return false;
                }

                item.ReservedQuantity += quantity;
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Estoque reservado - ProductId: {ProductId}, Quantidade: {Quantity}, Motivo: {Reason}", 
                    productId, quantity, reason);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reservar estoque - ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> ReleaseReservedStockAsync(Guid productId, int quantity, string reason = "")
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (item == null)
                {
                    _logger.LogWarning("Produto não encontrado no inventário: {ProductId}", productId);
                    return false;
                }

                if (item.ReservedQuantity < quantity)
                {
                    _logger.LogWarning("Quantidade reservada insuficiente - ProductId: {ProductId}, Reservado: {Reserved}, Solicitado: {Requested}", 
                        productId, item.ReservedQuantity, quantity);
                    return false;
                }

                item.ReservedQuantity -= quantity;
                item.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reserva liberada - ProductId: {ProductId}, Quantidade: {Quantity}, Motivo: {Reason}", 
                    productId, quantity, reason);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao liberar reserva - ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<StockCheckDto?> CheckStockAsync(Guid productId)
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                if (item == null)
                {
                    return null;
                }

                return new StockCheckDto
                {
                    ProductId = item.ProductId,
                    SKU = item.SKU,
                    AvailableQuantity = item.AvailableQuantity,
                    IsAvailable = item.AvailableQuantity > 0,
                    IsLowStock = item.IsLowStock,
                    IsOutOfStock = item.IsOutOfStock
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar estoque - ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<StockCheckDto?> CheckStockBySkuAsync(string sku)
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.SKU == sku);

                if (item == null)
                {
                    return null;
                }

                return new StockCheckDto
                {
                    ProductId = item.ProductId,
                    SKU = item.SKU,
                    AvailableQuantity = item.AvailableQuantity,
                    IsAvailable = item.AvailableQuantity > 0,
                    IsLowStock = item.IsLowStock,
                    IsOutOfStock = item.IsOutOfStock
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar estoque por SKU: {SKU}", sku);
                throw;
            }
        }

        public async Task<bool> IsStockAvailableAsync(Guid productId, int requiredQuantity)
        {
            try
            {
                var item = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == productId);

                return item != null && item.AvailableQuantity >= requiredQuantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar disponibilidade - ProductId: {ProductId}", productId);
                throw;
            }
        }

        public async Task<IEnumerable<InventoryItemDto>> GetLowStockItemsAsync()
        {
            try
            {
                var items = await _context.InventoryItems
                    .Where(i => (i.Quantity - i.ReservedQuantity) <= i.MinimumQuantity)
                    .OrderBy(i => i.SKU)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<InventoryItemDto>>(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar itens com baixo estoque");
                throw;
            }
        }

        public async Task<IEnumerable<InventoryItemDto>> GetOutOfStockItemsAsync()
        {
            try
            {
                var items = await _context.InventoryItems
                    .Where(i => (i.Quantity - i.ReservedQuantity) <= 0)
                    .OrderBy(i => i.SKU)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<InventoryItemDto>>(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar itens fora de estoque");
                throw;
            }
        }

        public async Task<IEnumerable<InventoryItemDto>> GetInventoryByLocationAsync(string location)
        {
            try
            {
                var items = await _context.InventoryItems
                    .Where(i => i.Location.Contains(location))
                    .OrderBy(i => i.SKU)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<InventoryItemDto>>(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar inventário por localização: {Location}", location);
                throw;
            }
        }
    }
} 