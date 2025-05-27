using Order.API.DTOs;
using Order.API.Models;

namespace Order.API.Services
{
    public interface IOrderService
    {
        // Operações básicas de CRUD
        Task<IEnumerable<OrderSummaryDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto?> GetOrderByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByCustomerIdAsync(Guid customerId);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto?> UpdateOrderAsync(Guid id, UpdateOrderDto updateOrderDto);
        Task<bool> DeleteOrderAsync(Guid id);

        // Operações de status
        Task<OrderDto?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto updateStatusDto);
        Task<bool> CancelOrderAsync(Guid id, string reason = "");
        Task<bool> CompleteOrderAsync(Guid id);

        // Consultas especializadas
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByStatusAsync(OrderStatus status);
        Task<IEnumerable<OrderSummaryDto>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<OrderSummaryDto>> GetRecentOrdersAsync(int days = 7);
        Task<IEnumerable<OrderSummaryDto>> GetPendingOrdersAsync();
        Task<IEnumerable<OrderSummaryDto>> GetProcessingOrdersAsync();

        // Estatísticas
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalOrdersCountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetAverageOrderValueAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Validações
        Task<bool> CanCancelOrderAsync(Guid id);
        Task<bool> OrderExistsAsync(Guid id);
        Task<bool> OrderNumberExistsAsync(string orderNumber);
    }
} 