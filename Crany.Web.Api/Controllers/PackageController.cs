using Crany.Shared.Protos;
using Crany.Web.Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Crany.Web.Api.Controllers;

[Route("api/v{version:apiVersion}/package")]
[ApiController]
[ApiVersion("2.0", Deprecated = false)]
[ApiExplorerSettings(GroupName = "v2", IgnoreApi = false)]
public class PackageController(IPushPackageService pushPackageService, AuthService.AuthServiceClient client) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> PushPackage(
        [FromHeader(Name="X-NuGet-ApiKey")]string apiKey, 
        [FromForm(Name = "package")]IFormFile? packageFile)
    {
        if ( string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("No API key provided.");
        }
        
        var response = await client.GetUserIdByApiKeyAsync(new GetUserIdByApiKeyRequest()
        {
            ApiKey = apiKey
        });
        
        if(!response.IsValid) 
            return Unauthorized(response.Message);
        
        var (success, message) = await pushPackageService.PushPackageAsync(response.UserId, packageFile);
        if (!success)
            return BadRequest(message);

        return Ok(message);
    }
}