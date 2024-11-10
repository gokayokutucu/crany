using System.IO.Compression;
using Crany.Domain.Entities;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using File = Crany.Domain.Entities.File;

namespace Crany.Web.Api.Controllers;

[Route("api/v3/package")]
[ApiController]
public class PackageController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("test")]
    [Authorize]
    public async Task<IActionResult> GetTest()
    {
        
        return Ok("Test");
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
        var package = await context.Packages
            .Include(p => p.Dependencies)
            .Include(p => p.Files)
            .Include(p => p.UserPackages)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (package == null)
            return NotFound();
        
        return Ok(package);
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
        if (package == null)
            return NotFound();
        
        context.Entry(package).CurrentValues.SetValues(updatedPackage);
        await context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var package = await context.Packages.FindAsync(id);
        if (package == null)
            return NotFound();
        
        context.Packages.Remove(package);
        await context.SaveChangesAsync();
        
        return NoContent();
    }
    
     [HttpPost("push")]
    public async Task<IActionResult> PushPackage([FromForm] IFormFile? packageFile)
    {
        if (packageFile == null || packageFile.Length == 0)
            return BadRequest("No package file provided.");

        var savePath = Path.Combine("PackageStorage", packageFile.FileName);
        Directory.CreateDirectory("PackageStorage");

        await using var fileStream = new FileStream(savePath, FileMode.Create);
        await packageFile.CopyToAsync(fileStream);

        var (package, protoFiles) = ExtractPackageAndProtoFiles(savePath);
        if (package == null)
            return BadRequest("Failed to extract package metadata or proto files.");

        context.Packages.Add(package);
        foreach (var protoFile in protoFiles)
        {
            protoFile.Package = package;
            context.Files.Add(protoFile);
        }
        await context.SaveChangesAsync();

        return Ok("Package pushed successfully.");
    }

    private (Package? package, List<File> protoFiles) ExtractPackageAndProtoFiles(string packageFilePath)
    {
        try
        {
            using var archive = ZipFile.OpenRead(packageFilePath);
            var nuspecEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec"));
            if (nuspecEntry == null) return (null, new List<File>());

            using var nuspecStream = nuspecEntry.Open();
            using var reader = new StreamReader(nuspecStream);
            var nuspecContent = reader.ReadToEnd();

            var packageName = ExtractTagValue(nuspecContent, "id");
            var version = ExtractTagValue(nuspecContent, "version");
            var description = ExtractTagValue(nuspecContent, "description");
            
            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(version)) return (null, new List<File>());

            var versionParts = version.Split('.');
            int.TryParse(versionParts[0], out var majorVersion);
            int.TryParse(versionParts[1], out var minorVersion);
            int.TryParse(versionParts[2], out var patchVersion);

            var package = new Package
            {
                Name = packageName,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                PatchVersion = patchVersion,
                Description = description,
                CreatedDate = DateTime.UtcNow
            };

            // Export `.proto` files
            var protoFiles = new List<File>();
            foreach (var entry in archive.Entries.Where(e => e.FullName.EndsWith(".proto")))
            {
                using var protoStream = entry.Open();
                using var protoReader = new StreamReader(protoStream);
                var content = protoReader.ReadToEnd();

                var protoFile = new File
                {
                    FileName = entry.Name,
                    Content = content,
                    TargetPath = entry.FullName,
                    Package = package
                };

                protoFiles.Add(protoFile);
            }

            return (package, protoFiles);
        }
        catch
        {
            return (null, new List<File>());
        }
    }

    private static string ExtractTagValue(string xmlContent, string tagName)
    {
        var startTag = $"<{tagName}>";
        var endTag = $"</{tagName}>";

        var startIndex = xmlContent.IndexOf(startTag, StringComparison.Ordinal) + startTag.Length;
        var endIndex = xmlContent.IndexOf(endTag, StringComparison.Ordinal);

        if (startIndex < 0 || endIndex < 0) return string.Empty;
        return xmlContent[startIndex..endIndex];
    }
}