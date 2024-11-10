using Microsoft.AspNetCore.Identity;

namespace Crany.Auth.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string? ApiKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastPasswordChangeAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastFailedLoginAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public virtual ICollection<ApplicationUserClaim> Claims { get; set; }
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}