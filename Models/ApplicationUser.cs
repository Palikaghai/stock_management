using Microsoft.AspNetCore.Identity;

namespace SmartStock.Models;

public class ApplicationUser : IdentityUser
{
    public const string ProfilePictureClaimType = "smartstock:profile-picture";

    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
