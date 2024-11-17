namespace Crany.Web.Api.Services.Abstractions;

public interface IPushPackageService
{
    Task<(bool Success, string Message)> PushPackageAsync(string? userUid, IFormFile? packageFile);
}