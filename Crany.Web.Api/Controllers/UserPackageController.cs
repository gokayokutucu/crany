using Crany.Web.Api.Infrastructure.Context;
using Crany.Web.Api.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crany.Web.Api.Controllers;

[Route("api/v3/user/packages")]
[ApiController]
public class UserPackageController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserPackages(string userId)
    {
        var userPackages = await context.UserPackages
            .Where(up => up.UserId == userId)
            .Include(up => up.Package)
            .ToListAsync();
        
        return Ok(userPackages);
    }

    [HttpPost]
    public async Task<IActionResult> AddUserPackage([FromBody] UserPackage userPackage)
    {
        context.UserPackages.Add(userPackage);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetUserPackages), new { userId = userPackage.UserId }, userPackage);
    }

    [HttpDelete("{userPackageId}")]
    public async Task<IActionResult> RemoveUserPackage(int userPackageId)
    {
        var userPackage = await context.UserPackages.FindAsync(userPackageId);
        if (userPackage == null)
            return NotFound();
        
        context.UserPackages.Remove(userPackage);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}