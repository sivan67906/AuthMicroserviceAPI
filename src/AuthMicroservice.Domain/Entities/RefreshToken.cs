namespace AuthMicroservice.Domain.Entities;

/// <summary>
/// Refresh token entity for JWT token rotation with client tracking
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string UserAgent { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string CreatedByIp { get; set; } = null!;
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    // Computed properties
    public bool IsActive => RevokedAt == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Client Client { get; set; } = null!;
}
