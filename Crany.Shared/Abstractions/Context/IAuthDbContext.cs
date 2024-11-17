using Crany.Shared.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crany.Shared.Abstractions.Context;

public interface IAuthDbContext
{
    DbSet<ApplicationUser> Users { get; set; }
    DbSet<ApplicationRole> Roles { get; set; }
    DbSet<ApplicationUserClaim> UserClaims { get; set; }
    DbSet<ApplicationUserRole> UserRoles { get; set; }
    DbSet<IdentityUserLogin<string>> UserLogins { get; set; }
    DbSet<IdentityRoleClaim<string>> RoleClaims { get; set; }
    DbSet<IdentityUserToken<string>> UserTokens { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}