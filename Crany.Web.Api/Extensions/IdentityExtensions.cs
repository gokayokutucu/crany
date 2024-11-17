using System.Security.Claims;
using System.Security.Principal;

namespace Crany.Web.Api.Extensions;

public static class IdentityExtensions
{
    public static string? PublicId(this IIdentity identity)
    {
        if (identity is not ClaimsIdentity claimsIdentity) return default;
        
        var userIdClaim = claimsIdentity.FindFirst("uid");
        
        return userIdClaim?.Value;
    }
}