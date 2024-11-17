using System.Security.Cryptography;
using System.Text;

namespace Crany.Shared.Helpers;
public static class Encryption
{
    public static string CalculateChecksum(Stream fileStream)
    {
        using var sha256 = SHA256.Create();
        fileStream.Position = 0;
        var hashBytes = sha256.ComputeHash(fileStream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static string CalculateChecksum(string content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}