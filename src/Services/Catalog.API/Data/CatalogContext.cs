using Microsoft.EntityFrameworkCore;
using Catalog.API.Models;

namespace Catalog.API.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(e => e.SKU).IsUnique();
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            // Seed de dados iniciais
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Notebook Dell Inspiron",
                    Description = "Notebook com processador Intel Core i5, 8GB RAM, 256GB SSD",
                    Price = 3499.99m,
                    Category = "Eletrônicos",
                    SKU = "NB-DELL-001",
                    ImageUrl = "/images/notebook-dell.jpg"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Mouse Logitech MX Master 3",
                    Description = "Mouse sem fio com tecnologia Darkfield",
                    Price = 549.90m,
                    Category = "Periféricos",
                    SKU = "MS-LOG-001",
                    ImageUrl = "/images/mouse-logitech.jpg"
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Teclado Mecânico Razer",
                    Description = "Teclado mecânico RGB com switches tácteis",
                    Price = 799.99m,
                    Category = "Periféricos",
                    SKU = "KB-RAZ-001",
                    ImageUrl = "/images/teclado-razer.jpg"
                }
            );
        }
    }
} 