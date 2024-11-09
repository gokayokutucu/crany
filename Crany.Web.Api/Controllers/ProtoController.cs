using Crany.Web.Api.Infrastructure.Context;
using Crany.Web.Api.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using File = Crany.Web.Api.Infrastructure.Entities.File;

namespace Crany.Web.Api.Controllers;

[Route("api/v3/packages/{packageId}/protos")]
[ApiController]
public class ProtoController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProtoFiles(int packageId)
    {
        var protoFiles = await context.ProtoFiles
            .Where(p => p.PackageId == packageId)
            .ToListAsync();
        
        return Ok(protoFiles);
    }

    [HttpPost]
    public async Task<IActionResult> AddProtoFile(int packageId, [FromBody] File file)
    {
        file.PackageId = packageId;
        context.ProtoFiles.Add(file);
        await context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetProtoFiles), new { packageId = packageId }, file);
    }

    [HttpDelete("{protoFileId}")]
    public async Task<IActionResult> DeleteProtoFile(int packageId, int protoFileId)
    {
        var protoFile = await context.ProtoFiles
            .FirstOrDefaultAsync(p => p.Id == protoFileId && p.PackageId == packageId);
        if (protoFile == null)
            return NotFound();
        
        context.ProtoFiles.Remove(protoFile);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
}