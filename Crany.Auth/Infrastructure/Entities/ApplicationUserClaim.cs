using Microsoft.AspNetCore.Identity;

namespace Crany.Auth.Infrastructure.Entities;

public class ApplicationUserClaim : IdentityUserClaim<string>
{
    public virtual ApplicationUser User { get; set; }
}