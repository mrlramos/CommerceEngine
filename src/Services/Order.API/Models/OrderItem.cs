using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.API.Models
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductSku { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(500)]
        public string ProductDescription { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O preço unitário deve ser maior que zero")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O desconto deve ser maior ou igual a zero")]
        public decimal DiscountAmount { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        // Propriedades calculadas
        public decimal SubTotal => Quantity * UnitPrice;
        public decimal TotalPrice => SubTotal - DiscountAmount;
        public decimal DiscountPercentage => UnitPrice > 0 ? (DiscountAmount / (Quantity * UnitPrice)) * 100 : 0;
    }
} 