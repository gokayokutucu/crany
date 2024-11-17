using Crany.Shared.Enums;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using File = Crany.Shared.Entities.File;

namespace Crany.Web.Api.Controllers;

[Route("api/v{version:apiVersion}/packages/{packageId}/files")]
[ApiController]
[ApiVersion("3.0", Deprecated = false)]
[ApiExplorerSettings(GroupName = "v3", IgnoreApi = false)]
public class FilesController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFiles(int packageId)
    {
        var files = await context.Files
            .Where(f => f.PackageId == packageId)
            .Select(f => new
            {
                f.Id,
                f.FileName,
                f.Type,
                ContentOrPath = f.Type == FileType.Proto ? f.Content : f.TargetPath
            })
            .ToListAsync();

        return Ok(files);
    }

    [HttpPost]
    public async Task<IActionResult> AddFile(int packageId, [FromBody] File file)
    {
        file.PackageId = packageId;

        if (file.Type == FileType.Proto && string.IsNullOrWhiteSpace(file.Content))
        {
            return BadRequest("Proto files require content.");
        }

        if (file.Type == FileType.Dll && string.IsNullOrWhiteSpace(file.TargetPath))
        {
            return BadRequest("DLL files require a target path.");
        }

        context.Files.Add(file);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFiles), new { packageId }, file);
    }

    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile(int packageId, int fileId)
    {
        var file = await context.Files
            .FirstOrDefaultAsync(f => f.Id == fileId && f.PackageId == packageId);
        
        if (file == null)
            return NotFound();

        context.Files.Remove(file);
        await context.SaveChangesAsync();

        return NoContent();
    }
}