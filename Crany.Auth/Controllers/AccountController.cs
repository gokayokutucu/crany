using Crany.Shared.Abstractions.Context;
using Crany.Shared.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crany.Auth.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AccountController(IAuthDbContext context, IPasswordHasher<ApplicationUser> passwordHasher) : ControllerBase
{
    public record PasswordRequest(string Password);

    [HttpPost("GetApiKey")]
    public async Task<IActionResult> GetUserApiKey([FromBody] PasswordRequest request)
    {
        // Fetch user identity from the database based on email
        var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity!.Name);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        // Verify the provided password against the stored hash
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordVerificationResult != PasswordVerificationResult.Success)
        {
            return Unauthorized("Invalid password.");
        }

        // Return the API key in the response
        return Ok(new { ApiKey = user.ApiKey });
    }
}