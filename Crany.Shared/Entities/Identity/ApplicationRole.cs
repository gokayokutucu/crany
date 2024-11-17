using Microsoft.AspNetCore.Identity;

namespace Crany.Shared.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    // Navigation Property
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}