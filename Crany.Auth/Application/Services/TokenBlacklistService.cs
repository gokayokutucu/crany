using System.Security.Cryptography;
using System.Text;
using Crany.Auth.Application.Services.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Crany.Auth.Application.Services;

public class TokenBlacklistService(IMemoryCache memoryCache) : ITokenBlacklistService
{
    private readonly SHA256 _sha256 = SHA256.Create();

    public void BlacklistToken(string token, TimeSpan expiration)
    {
        var tokenHash = HashToken(token);
        memoryCache.Set(tokenHash, true, expiration);
    }

    public bool IsTokenBlacklisted(string token)
    {
        var tokenHash = HashToken(token);
        return memoryCache.TryGetValue(tokenHash, out _);
    }

    private string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashedBytes = _sha256.ComputeHash(tokenBytes);
        return Convert.ToBase64String(hashedBytes);
    }
}