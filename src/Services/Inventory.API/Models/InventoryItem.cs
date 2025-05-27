using System;
using System.ComponentModel.DataAnnotations;

namespace Inventory.API.Models
{
    public class InventoryItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade reservada deve ser maior ou igual a zero")]
        public int ReservedQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade mínima deve ser maior ou igual a zero")]
        public int MinimumQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade máxima deve ser maior que zero")]
        public int MaximumQuantity { get; set; }

        [StringLength(100)]
        public string Location { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedade calculada
        public int AvailableQuantity => Quantity - ReservedQuantity;

        // Propriedade para verificar se está em baixo estoque
        public bool IsLowStock => AvailableQuantity <= MinimumQuantity;

        // Propriedade para verificar se está fora de estoque
        public bool IsOutOfStock => AvailableQuantity <= 0;
    }
} 