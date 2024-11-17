using Crany.Shared.Entities.Identity;

namespace Crany.Auth.Services.Abstractions;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);
    Task<string> CreateAndStoreRefreshTokenAsync(ApplicationUser user);
}