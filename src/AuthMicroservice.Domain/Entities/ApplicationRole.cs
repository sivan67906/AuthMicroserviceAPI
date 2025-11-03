using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.Domain.Entities;

/// <summary>
/// Application role entity extending Identity role with Guid primary key
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
