using Crany.Domain.Entities;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Crany.Web.Api.Controllers;

[Route("api/v3/user/packages")]
[ApiController]
public class UserPackageController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserPackageController(ApplicationDbContext context)
    {
        _context = context;
    }

    private string GetUserIdFromToken()
    {
        return User.FindFirst("uid")?.Value ?? throw new UnauthorizedAccessException("User ID not found in token.");
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPackages()
    {
        var userId = GetUserIdFromToken();

        var userPackages = await _context.UserPackages
            .Where(up => up.UserId == userId)
            .Include(up => up.Package)
            .ToListAsync();
        
        return Ok(userPackages);
    }

    [HttpPost]
    public async Task<IActionResult> AddUserPackage([FromBody] UserPackage userPackage)
    {
        userPackage.UserId = GetUserIdFromToken();
        
        _context.UserPackages.Add(userPackage);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetUserPackages), userPackage);
    }

    [HttpDelete("{userPackageId}")]
    public async Task<IActionResult> RemoveUserPackage(int userPackageId)
    {
        var userId = GetUserIdFromToken();

        var userPackage = await _context.UserPackages
            .Where(up => up.Id == userPackageId && up.UserId == userId)
            .FirstOrDefaultAsync();

        if (userPackage == null)
            return NotFound();

        _context.UserPackages.Remove(userPackage);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}