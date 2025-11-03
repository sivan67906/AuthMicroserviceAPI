namespace AuthMicroservice.Domain.Entities;

/// <summary>
/// Address entity for user shipping and billing addresses
/// </summary>
public class Address
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}
