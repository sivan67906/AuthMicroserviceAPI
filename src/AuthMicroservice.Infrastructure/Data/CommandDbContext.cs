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
                ConcurrencyStamp = "c080cfc3-0f10-4e11-a4ef-4ea1e542af0e"
            },
            new ApplicationRole
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                Description = "Standard user role",
                ConcurrencyStamp = "b1334f8d-0fa9-4250-8825-cc7bc64e8832"
            },
            new ApplicationRole
            {
                Id = moderatorRoleId,
                Name = "Moderator",
                NormalizedName = "MODERATOR",
                Description = "Moderator role with limited admin permissions",
                ConcurrencyStamp = "884037d1-7e3e-4a1d-ba98-f34e1c35381b"
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
                PasswordHash = "AQAAAAIAAYagAAAAELT+1lSllaK+4s7JzY5bPt5CODuJLyyzQ6NGkwxraBxocsDSbGR8URoCDWKZAfJ/Dg==",
                SecurityStamp = "cdba3083-2766-4b4d-8528-60999c2e9cb3",
                ConcurrencyStamp = "95d065ce-1e76-4885-b564-b6547556afd5",
                FullName = "System Administrator",
                IsActive = true,
                IsEmailConfirmed = true,
                CreatedAt = new DateTime(2025, 11, 3, 5, 57, 55, 200, DateTimeKind.Utc).AddTicks(1019),
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
                CreatedAt = new DateTime(2025, 11, 3, 5, 57, 55, 200, DateTimeKind.Utc).AddTicks(2764)
            },
            new Client
            {
                ClientId = "android-client",
                ClientName = "Android Application",
                Description = "Android mobile client",
                IsActive = true,
                CreatedAt = new DateTime(2025, 11, 3, 5, 57, 55, 200, DateTimeKind.Utc).AddTicks(2854)
            },
            new Client
            {
                ClientId = "ios-client",
                ClientName = "iOS Application",
                Description = "iOS mobile client",
                IsActive = true,
                CreatedAt = new DateTime(2025, 11, 3, 5, 57, 55, 200, DateTimeKind.Utc).AddTicks(2856)
            }
        );
    }
}