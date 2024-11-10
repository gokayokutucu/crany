using Crany.Auth.Infrastructure.Entities;

namespace Crany.Auth.Application.Services.Abstractions;

public interface ITokenService
{
    string CreateToken(ApplicationUser user, IList<string> roles);
    Task<string> CreateAndStoreRefreshTokenAsync(ApplicationUser user);
}