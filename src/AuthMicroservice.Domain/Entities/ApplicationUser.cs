using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.Domain.Entities;

/// <summary>
/// Application user entity extending Identity user with Guid primary key
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FullName { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public bool IsEmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? TwoFactorSecret { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    
    // Navigation properties
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
