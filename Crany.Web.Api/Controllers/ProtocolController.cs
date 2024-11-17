using System.Text.Json;
using System.Text.Json.Serialization;
using Crany.Shared.Enums;
using Crany.Shared.Models;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crany.Web.Api.Controllers;

[ApiVersionNeutral]
[Route("api")]
[ApiController]
public class ProtocolController(ApplicationDbContext context) : ControllerBase
{
    // /api/v3/index.json
    [HttpGet("v3/index.json")]
    [ProducesResponseType<object>(StatusCodes.Status200OK)]
    public IActionResult GetServiceIndex()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
        var schemaUrl = $"{baseUrl}/schema";

        var serviceIndex = new ServiceIndex
        {
            Version = "3.0.0",
            Resources =
            [
                new Resource
                {
                    Id = $"{baseUrl}/api/v3-flatcontainer/",
                    Type = "PackageBaseAddress/3.0.0",
                    Comment =
                        "Base URL of where NuGet packages are stored, in the format {baseUrl}/api/v3-flatcontainer/{id}/{version}/{filename}.nupkg"
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/registration5-semver1/{{id-lower}}/index.json",
                    Type = "PackageDisplayMetadataUriTemplate/3.0.0-rc",
                    Comment = "URI template used by NuGet Client to construct display metadata for Packages using ID"
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/registration5-semver1/{{id-lower}}/{{version-lower}}.json",
                    Type = "PackageVersionDisplayMetadataUriTemplate/3.0.0-rc",
                    Comment =
                        "URI template used by NuGet Client to construct display metadata for Packages using ID, Version"
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/packages/{{id}}/{{version}}",
                    Type = "PackageDetailsUriTemplate/5.1.0",
                    Comment = "URI template used by NuGet Client to construct details URL for packages"
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/profiles/{{owner}}",
                    Type = "OwnerDetailsUriTemplate/6.11.0",
                    Comment = "URI template used by NuGet Client to construct owner URL for packages"
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/catalog0/index.json",
                    Type = "Catalog/3.0.0",
                    Comment = "Index of the NuGet package catalog."
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/flatcontainer/{{id}}/{{version}}/{{id}}.{{version}}.nupkg",
                    Type = "PackageDownloadUriTemplate/3.0.0",
                    Comment = "Download URL for specific package version in the format {id}/{version}/{filename}.nupkg."
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/query",
                    Type = "SearchQueryService/3.0.0",
                    Comment = "Search packages by name or description."
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/flatcontainer/{{id}}/index.json",
                    Type = "PackageVersionsUriTemplate/3.0.0",
                    Comment = "URL for fetching all versions of a specific package."
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/stats/{{id}}",
                    Type = "StatisticsUriTemplate/3.0.0",
                    Comment = "URL for retrieving download count and statistics of a package."
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v2/package",
                    Type = "PackagePublish/2.0.0"
                }
            ],
            Context = new Context
            {
                Vocab = $"{schemaUrl}/services#",
                Comment = "http://www.w3.org/2000/01/rdf-schema#comment"
            }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return new JsonResult(serviceIndex, options);
    }

    // [HttpGet("catalog0/index.json")]
    // public async Task<IActionResult> GetCatalogIndex()
    // {
    //     var baseUrl = $"{Request.Scheme}://{Request.Host.Value}";
    //     var schemaUrl = $"{Request.Scheme}://{Request.Host.Value}/schema";
    //
    //     var catalogPages = await context.CatalogPages
    //         .Select(page => new
    //         {
    //             @id = $"{baseUrl}/api/v3/catalog0/page{page.Id}.json",
    //             @type = "CatalogPage",
    //             commitId = page.CommitId,
    //             commitTimeStamp = page.CommitTimeStamp.ToString("o"),
    //             count = page.ItemCount
    //         })
    //         .ToListAsync();
    //
    //     var catalog = new
    //     {
    //         @id = $"{baseUrl}/api/v3/catalog0/index.json",
    //         @type = new[] { "CatalogRoot", "AppendOnlyCatalog", "Permalink" },
    //         commitId = Guid.NewGuid().ToString(),
    //         commitTimeStamp = DateTime.UtcNow.ToString("o"),
    //         count = catalogPages.Count,
    //         nuget_lastCreated = DateTime.UtcNow.AddHours(-5).ToString("o"),
    //         nuget_lastDeleted = DateTime.UtcNow.AddHours(-12).ToString("o"),
    //         nuget_lastEdited = DateTime.UtcNow.AddHours(-5).ToString("o"),
    //         items = catalogPages,
    //         @context = new
    //         {
    //             @vocab = $"{schemaUrl}/catalog#",
    //             nuget = $"{schemaUrl}#",
    //             items = new { @id = "item", @container = "@set" },
    //             parent = new { @type = "@id" },
    //             commitTimeStamp = new { @type = "http://www.w3.org/2001/XMLSchema#dateTime" },
    //             nuget_lastCreated = new { @type = "http://www.w3.org/2001/XMLSchema#dateTime" },
    //             nuget_lastEdited = new { @type = "http://www.w3.org/2001/XMLSchema#dateTime" },
    //             nuget_lastDeleted = new { @type = "http://www.w3.org/2001/XMLSchema#dateTime" }
    //         }
    //     };
    //
    //     Response.ContentType = "application/json";
    //     return Ok(catalog);
    // }

    [HttpGet("flatcontainer/{id}/{version}/{filename}.nupkg")]
    public async Task<IActionResult> DownloadPackageFile(string id, string version, string filename)
    {
        // Normalize inputs for case-insensitivity
        var normalizedId = id.ToLower();
        var normalizedVersion = version.ToLower();
        var normalizedFilename = filename.ToLower();

        // Ensure the filename matches the package ID and version
        if (normalizedFilename != $"{normalizedId}.{normalizedVersion}.nupkg")
        {
            return BadRequest("Filename should match the package ID and version in the format 'id.version.nupkg'.");
        }

        // Query the file associated with the package ID and version
        var file = await context.Files
            .Join(
                context.Packages,
                file => file.PackageId,
                package => package.Id,
                (file, package) => new { file, package }
            )
            .Where(fp =>
                fp.package.Name.ToLower() == normalizedId &&
                $"{fp.package.MajorVersion}.{fp.package.MinorVersion}.{fp.package.PatchVersion}".ToLower() ==
                normalizedVersion &&
                fp.file.Type == FileType.Nupkg) // Ensures we are fetching the correct file type
            .Select(fp => fp.file)
            .FirstOrDefaultAsync();

        if (file == null)
        {
            return NotFound(new { Message = "Package file not found for the given ID and version." });
        }

        // Validate the physical file path exists
        if (!System.IO.File.Exists(file.TargetPath))
        {
            return NotFound(new { Message = "Physical file not found on the server." });
        }

        return PhysicalFile(
            file.TargetPath,
            "application/octet-stream",
            $"{normalizedId}.{normalizedVersion}.nupkg"
        );
    }

    [HttpGet("query")]
    public async Task<IActionResult> SearchQuery([FromQuery] string q, [FromQuery] bool prerelease = false)
    {
        var packages = await context.Packages
            .Where(p => (p.Name.Contains(q) || (p.Description != null && p.Description.Contains(q))))
            .Where(p => prerelease || p.PreReleaseTag == null)
            .ToListAsync();

        return Ok(packages);
    }

    // Package Versions Endpoint
    [HttpGet("flatcontainer/{id}/index.json")]
    public async Task<IActionResult> GetPackageVersions(int id)
    {
        var packageVersions = await context.Packages
            .Where(p => p.Id == id)
            .Select(p => $"{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}")
            .ToListAsync();

        if (!packageVersions.Any())
            return NotFound();

        Response.ContentType = "application/json";

        return Ok(packageVersions);
    }

    // Download Count and Statistics Endpoint (Optional)
    [HttpGet("stats/{id}")]
    public async Task<IActionResult> GetPackageStatistics(int id)
    {
        var package = await context.Packages
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null)
            return NotFound();

        var stats = new
        {
            package.Id,
            package.Name,
            package.DownloadCount,
            LastUpdated = package.CreatedDate
        };

        return Ok(stats);
    }
}