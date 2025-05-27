using System.ComponentModel.DataAnnotations;

namespace Order.API.Models
{
    public class Order
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor total deve ser maior que zero")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O subtotal deve ser maior que zero")]
        public decimal SubTotal { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O valor do frete deve ser maior ou igual a zero")]
        public decimal ShippingCost { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O valor do desconto deve ser maior ou igual a zero")]
        public decimal DiscountAmount { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        // Endereço de entrega
        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingCity { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ShippingState { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ShippingZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ShippingCountry { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Relacionamentos
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Propriedades calculadas
        public int TotalItems => OrderItems?.Sum(x => x.Quantity) ?? 0;
        public bool IsCompleted => Status == OrderStatus.Completed;
        public bool IsCancelled => Status == OrderStatus.Cancelled;
        public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Processing;
    }

    public enum OrderStatus
    {
        Pending = 1,        // Pendente
        Processing = 2,     // Processando
        Shipped = 3,        // Enviado
        Delivered = 4,      // Entregue
        Completed = 5,      // Concluído
        Cancelled = 6       // Cancelado
    }
} 