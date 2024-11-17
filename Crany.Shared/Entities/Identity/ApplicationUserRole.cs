using Microsoft.AspNetCore.Identity;

namespace Crany.Shared.Entities.Identity;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public virtual ApplicationUser User { get; set; }
    public virtual ApplicationRole Role { get; set; }
}