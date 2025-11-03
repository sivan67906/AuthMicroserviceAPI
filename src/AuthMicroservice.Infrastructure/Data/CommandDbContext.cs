using AuthMicroservice.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Data;

/// <summary>
/// Command DbContext for write operations using SQL Server
/// </summary>
public class CommandDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
    {
    }

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(typeof(CommandDbContext).Assembly);

        // Seed data
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed Roles
        var adminRoleId = Guid.Parse("8e445865-a24d-4543-a6c6-9443d048cdb9");
        var userRoleId = Guid.Parse("2c5e174e-3b0e-446f-86af-483d56fd7210");
        var moderatorRoleId = Guid.Parse("3a7e174e-3b0e-446f-86af-483d56fd7211");

        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                Description = "Administrator role with full permissions",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new ApplicationRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standard user role",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new ApplicationRole
            {
                Id = moderatorRoleId,
                Name = "Moderator",
                NormalizedName = "MODERATOR",
                Description = "Moderator role with limited admin permissions",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );

        // Seed Admin User
        var adminUserId = Guid.Parse("69bd714f-9576-45ba-b5b7-f00649be00de");
        var hasher = new PasswordHasher<ApplicationUser>();
        
        builder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = adminUserId,
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null!, "Admin@123"),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                FullName = "System Administrator",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsTwoFactorEnabled = false
            }
        );

        // Assign Admin role to Admin user
        builder.Entity<IdentityUserRole<Guid>>().HasData(
            new IdentityUserRole<Guid>
            {
                RoleId = adminRoleId,
                UserId = adminUserId
            }
        );

        // Seed Clients
        builder.Entity<Client>().HasData(
            new Client
            {
                ClientId = "web-client",
                ClientName = "Web Application",
                Description = "Web browser client",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                ClientId = "android-client",
                ClientName = "Android Application",
                Description = "Android mobile client",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                ClientId = "ios-client",
                ClientName = "iOS Application",
                Description = "iOS mobile client",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
