using Microsoft.AspNetCore.Mvc;
using Inventory.API.DTOs;
using Inventory.API.Services;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os itens de inventário
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAllInventoryItems()
        {
            var items = await _inventoryService.GetAllInventoryItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Obtém um item de inventário por ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryItemDto>> GetInventoryItem(Guid id)
        {
            var item = await _inventoryService.GetInventoryItemByIdAsync(id);
            
            if (item == null)
            {
                return NotFound($"Item de inventário com ID {id} não encontrado.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Obtém um item de inventário por Product ID
        /// </summary>
        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryItemDto>> GetInventoryItemByProductId(Guid productId)
        {
            var item = await _inventoryService.GetInventoryItemByProductIdAsync(productId);
            
            if (item == null)
            {
                return NotFound($"Item de inventário para produto {productId} não encontrado.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Obtém um item de inventário por SKU
        /// </summary>
        [HttpGet("sku/{sku}")]
        [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryItemDto>> GetInventoryItemBySku(string sku)
        {
            var item = await _inventoryService.GetInventoryItemBySkuAsync(sku);
            
            if (item == null)
            {
                return NotFound($"Item de inventário com SKU {sku} não encontrado.");
            }

            return Ok(item);
        }

        /// <summary>
        /// Cria um novo item de inventário
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryItemDto>> CreateInventoryItem([FromBody] CreateInventoryItemDto createInventoryItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var item = await _inventoryService.CreateInventoryItemAsync(createInventoryItemDto);
                return CreatedAtAction(nameof(GetInventoryItem), new { id = item.Id }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar item de inventário");
                return StatusCode(500, "Erro interno ao criar item de inventário");
            }
        }

        /// <summary>
        /// Atualiza um item de inventário
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryItemDto>> UpdateInventoryItem(Guid id, [FromBody] UpdateInventoryItemDto updateInventoryItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var item = await _inventoryService.UpdateInventoryItemAsync(id, updateInventoryItemDto);
                
                if (item == null)
                {
                    return NotFound($"Item de inventário com ID {id} não encontrado.");
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar item de inventário {ItemId}", id);
                return StatusCode(500, "Erro interno ao atualizar item de inventário");
            }
        }

        /// <summary>
        /// Remove um item de inventário
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInventoryItem(Guid id)
        {
            try
            {
                var deleted = await _inventoryService.DeleteInventoryItemAsync(id);
                
                if (!deleted)
                {
                    return NotFound($"Item de inventário com ID {id} não encontrado.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar item de inventário {ItemId}", id);
                return StatusCode(500, "Erro interno ao deletar item de inventário");
            }
        }

        /// <summary>
        /// Adiciona estoque para um produto
        /// </summary>
        [HttpPost("stock/add")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddStock([FromBody] StockMovementDto stockMovement)
        {
            if (stockMovement.Quantity <= 0)
            {
                return BadRequest("A quantidade deve ser maior que zero.");
            }

            var success = await _inventoryService.AddStockAsync(stockMovement.ProductId, stockMovement.Quantity, stockMovement.Reason);
            
            if (!success)
            {
                return NotFound($"Produto {stockMovement.ProductId} não encontrado no inventário.");
            }

            return Ok(new { Message = "Estoque adicionado com sucesso." });
        }

        /// <summary>
        /// Remove estoque de um produto
        /// </summary>
        [HttpPost("stock/remove")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveStock([FromBody] StockMovementDto stockMovement)
        {
            if (stockMovement.Quantity <= 0)
            {
                return BadRequest("A quantidade deve ser maior que zero.");
            }

            var success = await _inventoryService.RemoveStockAsync(stockMovement.ProductId, stockMovement.Quantity, stockMovement.Reason);
            
            if (!success)
            {
                return BadRequest("Não foi possível remover o estoque. Verifique se o produto existe e se há estoque suficiente.");
            }

            return Ok(new { Message = "Estoque removido com sucesso." });
        }

        /// <summary>
        /// Reserva estoque para um produto
        /// </summary>
        [HttpPost("stock/reserve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReserveStock([FromBody] StockMovementDto stockMovement)
        {
            if (stockMovement.Quantity <= 0)
            {
                return BadRequest("A quantidade deve ser maior que zero.");
            }

            var success = await _inventoryService.ReserveStockAsync(stockMovement.ProductId, stockMovement.Quantity, stockMovement.Reason);
            
            if (!success)
            {
                return BadRequest("Não foi possível reservar o estoque. Verifique se o produto existe e se há estoque suficiente.");
            }

            return Ok(new { Message = "Estoque reservado com sucesso." });
        }

        /// <summary>
        /// Libera estoque reservado de um produto
        /// </summary>
        [HttpPost("stock/release")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReleaseReservedStock([FromBody] StockMovementDto stockMovement)
        {
            if (stockMovement.Quantity <= 0)
            {
                return BadRequest("A quantidade deve ser maior que zero.");
            }

            var success = await _inventoryService.ReleaseReservedStockAsync(stockMovement.ProductId, stockMovement.Quantity, stockMovement.Reason);
            
            if (!success)
            {
                return BadRequest("Não foi possível liberar a reserva. Verifique se o produto existe e se há reserva suficiente.");
            }

            return Ok(new { Message = "Reserva liberada com sucesso." });
        }

        /// <summary>
        /// Verifica o estoque de um produto
        /// </summary>
        [HttpGet("stock/check/{productId:guid}")]
        [ProducesResponseType(typeof(StockCheckDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockCheckDto>> CheckStock(Guid productId)
        {
            var stockCheck = await _inventoryService.CheckStockAsync(productId);
            
            if (stockCheck == null)
            {
                return NotFound($"Produto {productId} não encontrado no inventário.");
            }

            return Ok(stockCheck);
        }

        /// <summary>
        /// Verifica o estoque de um produto por SKU
        /// </summary>
        [HttpGet("stock/check/sku/{sku}")]
        [ProducesResponseType(typeof(StockCheckDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StockCheckDto>> CheckStockBySku(string sku)
        {
            var stockCheck = await _inventoryService.CheckStockBySkuAsync(sku);
            
            if (stockCheck == null)
            {
                return NotFound($"Produto com SKU {sku} não encontrado no inventário.");
            }

            return Ok(stockCheck);
        }

        /// <summary>
        /// Verifica se há estoque disponível para uma quantidade específica
        /// </summary>
        [HttpGet("stock/available/{productId:guid}/{quantity:int}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> IsStockAvailable(Guid productId, int quantity)
        {
            var isAvailable = await _inventoryService.IsStockAvailableAsync(productId, quantity);
            return Ok(isAvailable);
        }

        /// <summary>
        /// Obtém itens com baixo estoque
        /// </summary>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetLowStockItems()
        {
            var items = await _inventoryService.GetLowStockItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Obtém itens fora de estoque
        /// </summary>
        [HttpGet("out-of-stock")]
        [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetOutOfStockItems()
        {
            var items = await _inventoryService.GetOutOfStockItemsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Obtém inventário por localização
        /// </summary>
        [HttpGet("location/{location}")]
        [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventoryByLocation(string location)
        {
            var items = await _inventoryService.GetInventoryByLocationAsync(location);
            return Ok(items);
        }
    }
} 