namespace AuthMicroservice.Domain.Entities;

/// <summary>
/// Client entity to track different application clients (Web, Android, iOS, etc.)
/// </summary>
public class Client
{
    public string ClientId { get; set; } = null!;
    public string ClientName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
