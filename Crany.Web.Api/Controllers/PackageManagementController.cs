using Crany.Shared.Entities;
using Crany.Web.Api.Infrastructure.Context;
using Crany.Web.Api.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crany.Web.Api.Controllers;


[Route("api/v{version:apiVersion}/package")]
[ApiController]
[ApiVersion("3.0", Deprecated = false)]
[ApiExplorerSettings(GroupName = "v3", IgnoreApi = false)]
[Authorize]
public class PackageManagementController(ApplicationDbContext context, IPushPackageService pushPackageService) : BaseController
{
    [HttpPost("upload")]
    public async Task<IActionResult> PushPackage(IFormFile? packageFile)
    {
        var (success, message) = await pushPackageService.PushPackageAsync(PublicUserId, packageFile);
        if (!success)
            return BadRequest(message);

        return Ok(message);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllPackages()
    {
        var packages = await context.Packages.ToListAsync();

        return Ok(packages);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPackageById(int id)
    {
        // Fetch the package
        var package = await context.Packages.FirstOrDefaultAsync(p => p.Id == id);
        if (package == null) return NotFound();

        // Fetch related dependencies, files, and user packages
        var dependencies = await context.PackageDependencies
            .Where(d => d.PackageId == id)
            .ToListAsync();

        var files = await context.Files
            .Where(f => f.PackageId == id)
            .ToListAsync();

        var userPackages = await context.UserPackages
            .Where(up => up.PackageId == id)
            .ToListAsync();

        // Construct the response
        var response = new
        {
            Package = package,
            Dependencies = dependencies,
            Files = files,
            UserPackages = userPackages
        };

        return Ok(response);
    }

    [HttpGet("{id}/{packageVersion}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPackageById(int id, string packageVersion)
    {
        // Fetch the package
        var package = await context.Packages
            .FirstOrDefaultAsync(p => p.Id == id &&
                                      $"{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}" == packageVersion);
        if (package == null) return NotFound();

        // Fetch related dependencies, files, and user packages
        var dependencies = await context.PackageDependencies
            .Where(d => d.PackageId == id)
            .ToListAsync();

        var files = await context.Files
            .Where(f => f.PackageId == id)
            .ToListAsync();

        var userPackages = await context.UserPackages
            .Where(up => up.PackageId == id)
            .ToListAsync();

        // Construct the response
        var response = new
        {
            Package = package,
            Dependencies = dependencies,
            Files = files,
            UserPackages = userPackages
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePackage([FromBody] Package package)
    {
        context.Packages.Add(package);
        await context.SaveChangesAsync();
    
        return CreatedAtAction(nameof(GetPackageById), new { id = package.Id }, package);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePackage(int id, [FromBody] Package updatedPackage)
    {
        var package = await context.Packages.FindAsync(id);
        if (package == null) return NotFound();

        context.Entry(package).CurrentValues.SetValues(updatedPackage);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var package = await context.Packages.FindAsync(id);
        if (package == null) return NotFound();

        context.Packages.Remove(package);
        await context.SaveChangesAsync();

        return NoContent();
    }
}