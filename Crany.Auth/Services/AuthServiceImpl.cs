using Crany.Shared.Abstractions.Context;
using Crany.Shared.Protos;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Crany.Auth.Services;

public class AuthServiceImpl(IAuthDbContext dbContext) : AuthService.AuthServiceBase
{
    public override async Task<UserIdByApiKeyResponse> GetUserIdByApiKey(GetUserIdByApiKeyRequest request,
        ServerCallContext context)
    {
        var userUId = await dbContext.Users
            .Where(u => u.ApiKey == request.ApiKey && u.IsActive)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(userUId))
        {
            return new UserIdByApiKeyResponse
            {
                UserId = Guid.Empty.ToString(),
                Message = "Invalid API key.",
                IsValid = false
            };
        }

        return new UserIdByApiKeyResponse
        {
            UserId = userUId,
            Message = "API key is valid.",
            IsValid = true
        };
    }
}