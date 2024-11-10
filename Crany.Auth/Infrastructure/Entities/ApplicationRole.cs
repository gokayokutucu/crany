using Microsoft.AspNetCore.Identity;

namespace Crany.Auth.Infrastructure.Entities;

public class ApplicationRole : IdentityRole
{
    // Navigation Property
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}