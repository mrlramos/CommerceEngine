using Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurações para RefreshToken
            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.RevokedByIp).HasMaxLength(50);
                entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
                entity.Property(e => e.ReasonRevoked).HasMaxLength(200);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => e.UserId);
            });

            // Configurações para ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(50);
                entity.Property(e => e.ZipCode).HasMaxLength(20);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                entity.Ignore(e => e.FullName);
                entity.Ignore(e => e.HasAddress);
            });

            // Seed de dados
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed de Roles
            var adminRoleId = Guid.NewGuid().ToString();
            var userRoleId = Guid.NewGuid().ToString();
            var managerRoleId = Guid.NewGuid().ToString();

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                },
                new IdentityRole
                {
                    Id = managerRoleId,
                    Name = "Manager",
                    NormalizedName = "MANAGER",
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                }
            );

            // Seed de usuários
            var hasher = new PasswordHasher<ApplicationUser>();

            var adminUserId = Guid.NewGuid().ToString();
            var managerUserId = Guid.NewGuid().ToString();
            var userUserId = Guid.NewGuid().ToString();

            var adminUser = new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@commerceengine.com",
                NormalizedUserName = "ADMIN@COMMERCEENGINE.COM",
                Email = "admin@commerceengine.com",
                NormalizedEmail = "ADMIN@COMMERCEENGINE.COM",
                EmailConfirmed = true,
                FirstName = "Administrador",
                LastName = "Sistema",
                PhoneNumber = "+55 11 99999-9999",
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            var managerUser = new ApplicationUser
            {
                Id = managerUserId,
                UserName = "manager@commerceengine.com",
                NormalizedUserName = "MANAGER@COMMERCEENGINE.COM",
                Email = "manager@commerceengine.com",
                NormalizedEmail = "MANAGER@COMMERCEENGINE.COM",
                EmailConfirmed = true,
                FirstName = "Gerente",
                LastName = "Vendas",
                PhoneNumber = "+55 11 88888-8888",
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            managerUser.PasswordHash = hasher.HashPassword(managerUser, "Manager@123");

            var normalUser = new ApplicationUser
            {
                Id = userUserId,
                UserName = "user@commerceengine.com",
                NormalizedUserName = "USER@COMMERCEENGINE.COM",
                Email = "user@commerceengine.com",
                NormalizedEmail = "USER@COMMERCEENGINE.COM",
                EmailConfirmed = true,
                FirstName = "Usuário",
                LastName = "Teste",
                PhoneNumber = "+55 11 77777-7777",
                PhoneNumberConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            normalUser.PasswordHash = hasher.HashPassword(normalUser, "User@123");

            builder.Entity<ApplicationUser>().HasData(adminUser, managerUser, normalUser);

            // Seed de UserRoles
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = adminUserId
                },
                new IdentityUserRole<string>
                {
                    RoleId = managerRoleId,
                    UserId = managerUserId
                },
                new IdentityUserRole<string>
                {
                    RoleId = userRoleId,
                    UserId = userUserId
                }
            );
        }
    }
} 