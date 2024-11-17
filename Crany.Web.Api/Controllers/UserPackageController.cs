using Crany.Shared.Entities;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crany.Web.Api.Controllers;

[ApiVersionNeutral]
[Route("api/user/packages")]
[ApiController]
[Authorize]
public class UserPackageController(ApplicationDbContext context) : ControllerBase
{
    private string GetUserIdFromToken()
    {
        return User.FindFirst("uid")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPackages()
    {
        // Extract the user ID from the token (assuming a helper method is available)
        var userId = GetUserIdFromToken();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { Message = "User ID could not be retrieved from the token." });
        }

        // Query the UserPackages for the logged-in user
        var userPackages = await context.UserPackages
            .Where(up => up.UserId == userId)
            .Join(
                context.Packages,
                userPackage => userPackage.PackageId,
                package => package.Id,
                (userPackage, package) => new
                {
                    PackageId = package.Id,
                    PackageName = package.Name,
                    Version = $"{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}",
                    userPackage.IsOwner
                }
            )
            .ToListAsync();

        // Check if no packages are associated with the user
        if (userPackages.Count == 0)
        {
            return NotFound(new { Message = "No packages found for the current user." });
        }

        return Ok(userPackages);
    }

    [HttpPost]
    public async Task<IActionResult> AddUserPackage([FromBody] UserPackage userPackage)
    {
        userPackage.UserId = GetUserIdFromToken();
        
        context.UserPackages.Add(userPackage);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetUserPackages), userPackage);
    }

    [HttpDelete("{userPackageId}")]
    public async Task<IActionResult> RemoveUserPackage(int userPackageId)
    {
        var userId = GetUserIdFromToken();

        var userPackage = await context.UserPackages
            .Where(up => up.Id == userPackageId && up.UserId == userId)
            .FirstOrDefaultAsync();

        if (userPackage == null)
            return NotFound();

        context.UserPackages.Remove(userPackage);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}