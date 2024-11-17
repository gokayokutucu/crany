namespace Crany.Auth.Services.Abstractions;

public interface ITokenBlacklistService
{
    void BlacklistToken(string token, TimeSpan expiration);
    bool IsTokenBlacklisted(string token);
}