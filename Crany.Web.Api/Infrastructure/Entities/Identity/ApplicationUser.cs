namespace Crany.Web.Api.Infrastructure.Entities.Identity;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
}