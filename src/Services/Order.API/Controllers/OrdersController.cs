using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Order.API.Services;

namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todos os pedidos (resumo)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Obtém um pedido por ID (detalhado)
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            return Ok(order);
        }

        /// <summary>
        /// Obtém um pedido por número do pedido
        /// </summary>
        [HttpGet("number/{orderNumber}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByOrderNumberAsync(orderNumber);
            
            if (order == null)
            {
                return NotFound($"Pedido com número {orderNumber} não encontrado.");
            }

            return Ok(order);
        }

        /// <summary>
        /// Obtém pedidos por cliente
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByCustomer(Guid customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }

        /// <summary>
        /// Cria um novo pedido
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                return StatusCode(500, "Erro interno ao criar pedido");
            }
        }

        /// <summary>
        /// Atualiza um pedido
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> UpdateOrder(Guid id, [FromBody] UpdateOrderDto updateOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _orderService.UpdateOrderAsync(id, updateOrderDto);
                
                if (order == null)
                {
                    return NotFound($"Pedido com ID {id} não encontrado.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pedido {OrderId}", id);
                return StatusCode(500, "Erro interno ao atualizar pedido");
            }
        }

        /// <summary>
        /// Remove um pedido (apenas se estiver pendente)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                var deleted = await _orderService.DeleteOrderAsync(id);
                
                if (!deleted)
                {
                    return BadRequest("Não foi possível deletar o pedido. Verifique se existe e se está pendente.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar pedido {OrderId}", id);
                return StatusCode(500, "Erro interno ao deletar pedido");
            }
        }

        /// <summary>
        /// Atualiza o status de um pedido
        /// </summary>
        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto);
                
                if (order == null)
                {
                    return NotFound($"Pedido com ID {id} não encontrado.");
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido {OrderId}", id);
                return StatusCode(500, "Erro interno ao atualizar status do pedido");
            }
        }

        /// <summary>
        /// Cancela um pedido
        /// </summary>
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] string? reason = null)
        {
            try
            {
                var cancelled = await _orderService.CancelOrderAsync(id, reason ?? "");
                
                if (!cancelled)
                {
                    return BadRequest("Não foi possível cancelar o pedido. Verifique se existe e se pode ser cancelado.");
                }

                return Ok(new { Message = "Pedido cancelado com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pedido {OrderId}", id);
                return StatusCode(500, "Erro interno ao cancelar pedido");
            }
        }

        /// <summary>
        /// Completa um pedido
        /// </summary>
        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteOrder(Guid id)
        {
            try
            {
                var completed = await _orderService.CompleteOrderAsync(id);
                
                if (!completed)
                {
                    return BadRequest("Não foi possível completar o pedido. Verifique se existe.");
                }

                return Ok(new { Message = "Pedido completado com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao completar pedido {OrderId}", id);
                return StatusCode(500, "Erro interno ao completar pedido");
            }
        }

        /// <summary>
        /// Obtém pedidos por status
        /// </summary>
        [HttpGet("status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        /// <summary>
        /// Obtém pedidos pendentes
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetPendingOrders()
        {
            var orders = await _orderService.GetPendingOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Obtém pedidos em processamento
        /// </summary>
        [HttpGet("processing")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetProcessingOrders()
        {
            var orders = await _orderService.GetProcessingOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Obtém pedidos recentes
        /// </summary>
        [HttpGet("recent")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetRecentOrders([FromQuery] int days = 7)
        {
            var orders = await _orderService.GetRecentOrdersAsync(days);
            return Ok(orders);
        }

        /// <summary>
        /// Obtém pedidos por período
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("A data inicial deve ser menor que a data final.");
            }

            var orders = await _orderService.GetOrdersByDateRangeAsync(startDate, endDate);
            return Ok(orders);
        }

        /// <summary>
        /// Obtém estatísticas de vendas
        /// </summary>
        [HttpGet("statistics/sales")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<ActionResult> GetSalesStatistics(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var totalSales = await _orderService.GetTotalSalesAsync(startDate, endDate);
                var totalOrders = await _orderService.GetTotalOrdersCountAsync(startDate, endDate);
                var averageOrderValue = await _orderService.GetAverageOrderValueAsync(startDate, endDate);

                var statistics = new
                {
                    TotalSales = totalSales,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                    Period = new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de vendas");
                return StatusCode(500, "Erro interno ao obter estatísticas");
            }
        }

        /// <summary>
        /// Verifica se um pedido pode ser cancelado
        /// </summary>
        [HttpGet("{id:guid}/can-cancel")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CanCancelOrder(Guid id)
        {
            var canCancel = await _orderService.CanCancelOrderAsync(id);
            return Ok(canCancel);
        }

        /// <summary>
        /// Verifica se um pedido existe
        /// </summary>
        [HttpGet("{id:guid}/exists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> OrderExists(Guid id)
        {
            var exists = await _orderService.OrderExistsAsync(id);
            return Ok(exists);
        }
    }
} 