using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.DTOs;
using Order.API.Models;

namespace Order.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(OrderContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        #region Operações básicas de CRUD

        public async Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os pedidos");
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                return order != null ? _mapper.Map<OrderDto>(order) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedido por ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<OrderDto?> GetOrderByOrderNumberAsync(string orderNumber)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                return order != null ? _mapper.Map<OrderDto>(order) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedido por número: {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByCustomerIdAsync(Guid customerId)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos por cliente: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            try
            {
                var order = _mapper.Map<Models.Order>(createOrderDto);
                order.Id = Guid.NewGuid();
                order.OrderNumber = await GenerateOrderNumberAsync();
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                // Processar itens do pedido
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                foreach (var itemDto in createOrderDto.OrderItems)
                {
                    // Aqui você integraria com o Catalog.API para buscar informações do produto
                    // Por enquanto, vou simular os dados
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = itemDto.ProductId,
                        ProductSku = itemDto.ProductSku,
                        ProductName = $"Produto {itemDto.ProductSku}",
                        ProductDescription = $"Descrição do produto {itemDto.ProductSku}",
                        Quantity = itemDto.Quantity,
                        UnitPrice = 100.00m, // Seria obtido do Catalog.API
                        DiscountAmount = itemDto.DiscountAmount,
                        ImageUrl = "https://example.com/product.jpg",
                        CreatedAt = DateTime.UtcNow
                    };

                    subTotal += orderItem.SubTotal - orderItem.DiscountAmount;
                    orderItems.Add(orderItem);
                }

                order.SubTotal = subTotal;
                order.TotalAmount = subTotal + order.ShippingCost - order.DiscountAmount;
                order.OrderItems = orderItems;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pedido criado: {OrderId} - {OrderNumber}", order.Id, order.OrderNumber);
                
                // Recarregar o pedido com os itens para retornar
                var createdOrder = await GetOrderByIdAsync(order.Id);
                return createdOrder!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                throw;
            }
        }

        public async Task<OrderDto?> UpdateOrderAsync(Guid id, UpdateOrderDto updateOrderDto)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return null;
                }

                _mapper.Map(updateOrderDto, order);
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pedido atualizado: {OrderId}", id);
                return await GetOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pedido: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return false;
                }

                // Só permite deletar pedidos pendentes
                if (order.Status != OrderStatus.Pending)
                {
                    _logger.LogWarning("Tentativa de deletar pedido com status inválido: {OrderId} - Status: {Status}", 
                        id, order.Status);
                    return false;
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pedido removido: {OrderId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover pedido: {OrderId}", id);
                throw;
            }
        }

        #endregion

        #region Operações de status

        public async Task<OrderDto?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto updateStatusDto)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return null;
                }

                var oldStatus = order.Status;
                order.Status = updateStatusDto.Status;
                order.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(updateStatusDto.Notes))
                {
                    order.Notes = updateStatusDto.Notes;
                }

                // Atualizar timestamps específicos
                switch (updateStatusDto.Status)
                {
                    case OrderStatus.Completed:
                        order.CompletedAt = DateTime.UtcNow;
                        break;
                    case OrderStatus.Cancelled:
                        order.CancelledAt = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Status do pedido atualizado: {OrderId} - {OldStatus} -> {NewStatus}", 
                    id, oldStatus, updateStatusDto.Status);

                return await GetOrderByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> CancelOrderAsync(Guid id, string reason = "")
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return false;
                }

                if (!order.CanBeCancelled)
                {
                    _logger.LogWarning("Tentativa de cancelar pedido que não pode ser cancelado: {OrderId} - Status: {Status}", 
                        id, order.Status);
                    return false;
                }

                order.Status = OrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(reason))
                {
                    order.Notes = $"{order.Notes}\nCancelado: {reason}".Trim();
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pedido cancelado: {OrderId} - Motivo: {Reason}", id, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pedido: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> CompleteOrderAsync(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return false;
                }

                order.Status = OrderStatus.Completed;
                order.CompletedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pedido concluído: {OrderId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao concluir pedido: {OrderId}", id);
                throw;
            }
        }

        #endregion

        #region Consultas especializadas

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status == status)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos por status: {Status}", status);
                throw;
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<OrderSummaryDto>>(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos por período: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetRecentOrdersAsync(int days = 7)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);
                return await GetOrdersByDateRangeAsync(startDate, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos recentes: {Days} dias", days);
                throw;
            }
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetPendingOrdersAsync()
        {
            return await GetOrdersByStatusAsync(OrderStatus.Pending);
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetProcessingOrdersAsync()
        {
            return await GetOrdersByStatusAsync(OrderStatus.Processing);
        }

        #endregion

        #region Estatísticas

        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Orders.Where(o => o.Status == OrderStatus.Completed);

                if (startDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= endDate.Value);

                return await query.SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular total de vendas");
                throw;
            }
        }

        public async Task<int> GetTotalOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= endDate.Value);

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao contar total de pedidos");
                throw;
            }
        }

        public async Task<decimal> GetAverageOrderValueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _context.Orders.Where(o => o.Status != OrderStatus.Cancelled);

                if (startDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= endDate.Value);

                var orders = await query.ToListAsync();
                return orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular valor médio dos pedidos");
                throw;
            }
        }

        #endregion

        #region Validações

        public async Task<bool> CanCancelOrderAsync(Guid id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                return order?.CanBeCancelled ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se pedido pode ser cancelado: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> OrderExistsAsync(Guid id)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se pedido existe: {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> OrderNumberExistsAsync(string orderNumber)
        {
            try
            {
                return await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se número do pedido existe: {OrderNumber}", orderNumber);
                throw;
            }
        }

        #endregion

        #region Métodos auxiliares

        private async Task<string> GenerateOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastOrder = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith($"ORD-{year}-"))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastOrder != null)
            {
                var lastNumberPart = lastOrder.OrderNumber.Split('-').LastOrDefault();
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"ORD-{year}-{nextNumber:D3}";
        }

        #endregion
    }
} 