using Microsoft.AspNetCore.Identity;

namespace Crany.Shared.Entities.Identity;

public class ApplicationUserClaim : IdentityUserClaim<string>
{
    public virtual ApplicationUser User { get; set; }
}