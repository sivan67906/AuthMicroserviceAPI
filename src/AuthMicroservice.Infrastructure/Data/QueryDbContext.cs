using AuthMicroservice.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Infrastructure.Data;

/// <summary>
/// Query DbContext for read operations using PostgreSQL with NoTracking
/// </summary>
public class QueryDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(typeof(QueryDbContext).Assembly);
    }
}
