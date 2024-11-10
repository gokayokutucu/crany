namespace Crany.Auth.Application.Services.Abstractions;

public interface ITokenBlacklistService
{
    void BlacklistToken(string token, TimeSpan expiration);
    bool IsTokenBlacklisted(string token);
}