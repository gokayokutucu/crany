using Crany.Shared.Attributes;
using Crany.Auth.Models;
using Crany.Auth.Services.Abstractions;
using Crany.Shared.Entities.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Crany.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
        : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            user.ApiKey = Guid.NewGuid().ToString();
            await userManager.UpdateAsync(user);

            return Ok("User registered successfully.");
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromForm] string grant_type, [FromForm] string username,
            [FromForm] string password)
        {
            if (grant_type != "password")
            {
                return BadRequest("Invalid grant_type");
            }

            var user = await userManager.FindByNameAsync(username);
            if (user == null || !await userManager.CheckPasswordAsync(user, password))
            {
                return Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenService.CreateToken(user, roles);
            var refreshToken = await tokenService.CreateAndStoreRefreshTokenAsync(user);

            return Ok(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                expires_in = 1800,
                refresh_token = refreshToken
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid email.");

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            
            // Send email with token
            
            return Ok("Reset password email sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Invalid email.");

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Password reset successfully.");
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout([FromServices] ITokenBlacklistService tokenBlacklistService,
            [FromAuthorizationHeader] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Authorization header is missing or invalid.");
            }

            tokenBlacklistService.BlacklistToken(token, TimeSpan.FromMinutes(30));

            return Ok(new LogoutResponse
            {
                Message = "Logged out successfully.",
                LoggedOutAt = DateTime.UtcNow
            });
        }
    }
}