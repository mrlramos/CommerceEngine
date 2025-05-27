using Microsoft.EntityFrameworkCore;
using Order.API.Models;

namespace Order.API.Data
{
    public class OrderContext : DbContext
    {
        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }

        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela Order
            modelBuilder.Entity<Models.Order>(entity =>
            {
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);

                // Configuração de precisão decimal
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.SubTotal).HasPrecision(18, 2);
                entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);

                // Propriedades calculadas não são mapeadas para o banco
                entity.Ignore(e => e.TotalItems);
                entity.Ignore(e => e.IsCompleted);
                entity.Ignore(e => e.IsCancelled);
                entity.Ignore(e => e.CanBeCancelled);

                // Relacionamento com OrderItems
                entity.HasMany(e => e.OrderItems)
                      .WithOne(e => e.Order)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuração da tabela OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.ProductSku);

                // Configuração de precisão decimal
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);

                // Propriedades calculadas não são mapeadas para o banco
                entity.Ignore(e => e.SubTotal);
                entity.Ignore(e => e.TotalPrice);
                entity.Ignore(e => e.DiscountPercentage);
            });

            // Seed de dados iniciais
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // IDs fixos para os dados seed
            var order1Id = Guid.NewGuid();
            var order2Id = Guid.NewGuid();
            var order3Id = Guid.NewGuid();

            // Pedidos seed
            var orders = new[]
            {
                new Models.Order
                {
                    Id = order1Id,
                    OrderNumber = "ORD-2025-001",
                    CustomerId = Guid.NewGuid(),
                    CustomerName = "João Silva",
                    CustomerEmail = "joao.silva@email.com",
                    Status = OrderStatus.Completed,
                    TotalAmount = 1299.90m,
                    SubTotal = 1249.90m,
                    ShippingCost = 50.00m,
                    DiscountAmount = 0.00m,
                    Notes = "Primeira compra do cliente",
                    ShippingAddress = "Rua das Flores, 123",
                    ShippingCity = "São Paulo",
                    ShippingState = "SP",
                    ShippingZipCode = "01234-567",
                    ShippingCountry = "Brasil",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3),
                    CompletedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Models.Order
                {
                    Id = order2Id,
                    OrderNumber = "ORD-2025-002",
                    CustomerId = Guid.NewGuid(),
                    CustomerName = "Maria Santos",
                    CustomerEmail = "maria.santos@email.com",
                    Status = OrderStatus.Processing,
                    TotalAmount = 799.80m,
                    SubTotal = 749.80m,
                    ShippingCost = 50.00m,
                    DiscountAmount = 0.00m,
                    Notes = "Entrega expressa solicitada",
                    ShippingAddress = "Av. Paulista, 1000",
                    ShippingCity = "São Paulo",
                    ShippingState = "SP",
                    ShippingZipCode = "01310-100",
                    ShippingCountry = "Brasil",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Models.Order
                {
                    Id = order3Id,
                    OrderNumber = "ORD-2025-003",
                    CustomerId = Guid.NewGuid(),
                    CustomerName = "Pedro Costa",
                    CustomerEmail = "pedro.costa@email.com",
                    Status = OrderStatus.Pending,
                    TotalAmount = 549.90m,
                    SubTotal = 549.90m,
                    ShippingCost = 0.00m,
                    DiscountAmount = 0.00m,
                    Notes = "Frete grátis aplicado",
                    ShippingAddress = "Rua do Comércio, 456",
                    ShippingCity = "Rio de Janeiro",
                    ShippingState = "RJ",
                    ShippingZipCode = "20040-020",
                    ShippingCountry = "Brasil",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    UpdatedAt = DateTime.UtcNow.AddHours(-6)
                }
            };

            // Itens dos pedidos seed
            var orderItems = new[]
            {
                // Itens do Pedido 1
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order1Id,
                    ProductId = Guid.NewGuid(),
                    ProductSku = "NB-DELL-001",
                    ProductName = "Notebook Dell Inspiron 15",
                    ProductDescription = "Notebook Dell com processador Intel i5",
                    Quantity = 1,
                    UnitPrice = 1249.90m,
                    DiscountAmount = 0.00m,
                    ImageUrl = "https://example.com/notebook-dell.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                
                // Itens do Pedido 2
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2Id,
                    ProductId = Guid.NewGuid(),
                    ProductSku = "MS-LOG-001",
                    ProductName = "Mouse Logitech MX Master 3",
                    ProductDescription = "Mouse sem fio com tecnologia Darkfield",
                    Quantity = 1,
                    UnitPrice = 549.90m,
                    DiscountAmount = 0.00m,
                    ImageUrl = "https://example.com/mouse-logitech.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order2Id,
                    ProductId = Guid.NewGuid(),
                    ProductSku = "KB-RAZ-001",
                    ProductName = "Teclado Razer BlackWidow V3",
                    ProductDescription = "Teclado mecânico para gaming",
                    Quantity = 1,
                    UnitPrice = 199.90m,
                    DiscountAmount = 0.00m,
                    ImageUrl = "https://example.com/teclado-razer.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                
                // Itens do Pedido 3
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order3Id,
                    ProductId = Guid.NewGuid(),
                    ProductSku = "MS-LOG-001",
                    ProductName = "Mouse Logitech MX Master 3",
                    ProductDescription = "Mouse sem fio com tecnologia Darkfield",
                    Quantity = 1,
                    UnitPrice = 549.90m,
                    DiscountAmount = 0.00m,
                    ImageUrl = "https://example.com/mouse-logitech.jpg",
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                }
            };

            modelBuilder.Entity<Models.Order>().HasData(orders);
            modelBuilder.Entity<OrderItem>().HasData(orderItems);
        }
    }
} 