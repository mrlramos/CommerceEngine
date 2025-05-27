namespace Inventory.API.DTOs
{
    public class InventoryItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int MaximumQuantity { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateInventoryItemDto
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int MaximumQuantity { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class UpdateInventoryItemDto
    {
        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int MaximumQuantity { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class StockMovementDto
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string MovementType { get; set; } = string.Empty; // "IN", "OUT", "RESERVE", "RELEASE"
        public string Reason { get; set; } = string.Empty;
    }

    public class StockCheckDto
    {
        public Guid ProductId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOutOfStock { get; set; }
    }
} 