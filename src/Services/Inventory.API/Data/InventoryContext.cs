using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;

namespace Inventory.API.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options) : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela InventoryItem
            modelBuilder.Entity<InventoryItem>(entity =>
            {
                entity.HasIndex(e => e.ProductId).IsUnique();
                entity.HasIndex(e => e.SKU).IsUnique();
                
                // Propriedades calculadas não são mapeadas para o banco
                entity.Ignore(e => e.AvailableQuantity);
                entity.Ignore(e => e.IsLowStock);
                entity.Ignore(e => e.IsOutOfStock);
            });

            // Seed de dados iniciais
            var inventoryItems = new[]
            {
                new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    SKU = "NB-DELL-001",
                    Quantity = 50,
                    ReservedQuantity = 5,
                    MinimumQuantity = 10,
                    MaximumQuantity = 100,
                    Location = "Estoque A - Prateleira 1"
                },
                new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    SKU = "MS-LOG-001",
                    Quantity = 25,
                    ReservedQuantity = 2,
                    MinimumQuantity = 5,
                    MaximumQuantity = 50,
                    Location = "Estoque B - Prateleira 3"
                },
                new InventoryItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    SKU = "KB-RAZ-001",
                    Quantity = 8,
                    ReservedQuantity = 1,
                    MinimumQuantity = 10,
                    MaximumQuantity = 30,
                    Location = "Estoque A - Prateleira 2"
                }
            };

            modelBuilder.Entity<InventoryItem>().HasData(inventoryItems);
        }
    }
} 