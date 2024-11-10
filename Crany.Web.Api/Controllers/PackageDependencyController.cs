using Crany.Domain.Entities;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crany.Web.Api.Controllers;

[Route("api/v3/packages/{packageId}/dependencies")]
[ApiController]
public class PackageDependencyController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDependencies(int packageId)
    {
        var dependencies = await context.PackageDependencies
            .Where(d => d.PackageId == packageId)
            .ToListAsync();
        return Ok(dependencies);
    }

    [HttpPost]
    public async Task<IActionResult> AddDependency(int packageId, [FromBody] PackageDependency dependency)
    {
        dependency.PackageId = packageId;
        context.PackageDependencies.Add(dependency);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetDependencies), new { packageId = packageId }, dependency);
    }

    [HttpDelete("{dependencyId}")]
    public async Task<IActionResult> RemoveDependency(int packageId, int dependencyId)
    {
        var dependency = await context.PackageDependencies
            .FirstOrDefaultAsync(d => d.DependencyId == dependencyId && d.PackageId == packageId);
        if (dependency == null)
            return NotFound();
        
        context.PackageDependencies.Remove(dependency);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}