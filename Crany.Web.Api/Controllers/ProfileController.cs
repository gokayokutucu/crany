using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Crany.Web.Api.Infrastructure.Context;

namespace Crany.Web.Api.Controllers;

[ApiVersionNeutral]
[Route("api/profiles")]
[ApiController]
public class ProfileController(ApplicationDbContext context) : ControllerBase
{
    // /api/profiles/{owner}
    [HttpGet("{userUid}")]
    public async Task<IActionResult> GetProfileByOwner(string userUid)
    {
        // Fetch user packages for the specified owner
        var userPackages = await context.UserPackages
            .Where(up => up.UserId == userUid) // owner is the user ID
            .ToListAsync();

        if (userPackages.Count == 0)
        {
            return NotFound(new { Message = "Profile or packages not found for the specified owner." });
        }

        // Fetch related package information for user packages
        var packageIds = userPackages.Select(up => up.PackageId).Distinct().ToList();
        var packages = await context.Packages
            .Where(p => packageIds.Contains(p.Id))
            .ToListAsync();

        // Construct the response
        var packageDetails = userPackages
            .Select(up => new
            {
                PackageId = up.PackageId,
                PackageName = packages.Find(p => p.Id == up.PackageId)?.Name,
                Version = packages.Find(p => p.Id == up.PackageId) is { } pkg 
                    ? $"{pkg.MajorVersion}.{pkg.MinorVersion}.{pkg.PatchVersion}" 
                    : "Unknown",
                up.IsOwner
            })
            .ToList();

        var profile = new
        {
            Owner = userUid,
            Packages = packageDetails
        };

        return Ok(profile);
    }
}