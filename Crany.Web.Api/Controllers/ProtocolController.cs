using System.Text.Json;
using System.Text.Json.Serialization;
using Crany.Shared.Enums;
using Crany.Shared.Models;
using Crany.Shared.Models.PackageInfos;
using Crany.Web.Api.Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PackageVersionRoot = Crany.Shared.Models.RegistrationIndex.PackageVersionRoot;

namespace Crany.Web.Api.Controllers;

[ApiVersionNeutral]
[Route("api")]
[ApiController]
public class ProtocolController(ApplicationDbContext context, ILogger<ProtocolController> logger) : ControllerBase
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
                    Id = $"{baseUrl}/api/v3/registration5-semver1/",
                    Type = "RegistrationsBaseUrl/3.6.0",
                    Comment = "Base URL of registration blobs"
                },
                new Resource
                {
                    Id = $"{baseUrl}/api/v3/registration5-gz-semver2/",
                    Type = "RegistrationsBaseUrl/3.6.0",
                    Comment = "Base URL of compressed registration blobs supporting SemVer 2.0.0."
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
                    Id = $"{baseUrl}/api/v3-flatcontainer/{{id}}/index.json",
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

    [HttpGet("v3-flatcontainer/{id}/{version}/{filename}")]
    public async Task<IActionResult> DownloadPackageFile(string id, string version, string filename)
    {
        // Normalize inputs for case-insensitivity
        var normalizedId = id.ToLowerInvariant();
        var normalizedVersion = version.ToLowerInvariant();
        var normalizedFilename = filename.ToLowerInvariant();

        // Ensure the filename matches the package ID and version
        if (normalizedFilename != $"{normalizedId}.{normalizedVersion}.nupkg")
        {
            return BadRequest(new
                { Message = "Filename should match the package ID and version in the format 'id.version.nupkg'." });
        }

        // Parse version numbers
        if (!TryParseVersion(normalizedVersion, out var majorVersion, out var minorVersion, out var patchVersion))
        {
            return BadRequest(new { Message = "Invalid version format. Expected format: 'major.minor.patch'." });
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
                fp.package.MajorVersion == majorVersion &&
                fp.package.MinorVersion == minorVersion &&
                fp.package.PatchVersion == patchVersion &&
                fp.file.Type == FileType.Nupkg)
            .Select(fp => fp.file)
            .FirstOrDefaultAsync();

        if (file == null)
        {
            return NotFound(new { Message = $"Package file not found for ID '{id}' and version '{version}'." });
        }

        // Validate the physical file path exists
        if (!System.IO.File.Exists(file.TargetPath))
        {
            logger.LogError("Physical file not found: {FilePath}", file.TargetPath);
            return NotFound(new
                { Message = $"Physical file not found on the server for ID '{id}' and version '{version}'." });
        }

        logger.LogInformation("Serving package file: {FilePath}", file.TargetPath);

        // Return the file for download
        return PhysicalFile(
            file.TargetPath,
            "application/octet-stream",
            file.FileName
        );
    }

    private bool TryParseVersion(string version, out int major, out int minor, out int patch)
    {
        major = minor = patch = 0;

        var parts = version.Split('.');
        if (parts.Length != 3 ||
            !int.TryParse(parts[0], out major) ||
            !int.TryParse(parts[1], out minor) ||
            !int.TryParse(parts[2], out patch))
        {
            return false;
        }

        return true;
    }

    [HttpGet("v3/registration5-semver1")]
    public async Task<IActionResult> GetRegistrationIndex()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // Paketleri çek
        var packages = await context.Packages.ToListAsync();

        // PackageTag ve Tag verilerini ayrı olarak çek
        var packageTags = await context.PackageTags.ToListAsync();
        var tags = await context.Tags.ToListAsync();

        // Dependencies verisini çek
        var dependencies = await context.PackageDependencies.ToListAsync();

        // Paketlerin detaylarını oluştur
        var packageItems = packages
            .GroupBy(p => p.Name.ToLower())
            .Select(group => new Crany.Shared.Models.RegistrationIndex.PackageVersionGroup
            {
                Id = $"{baseUrl}/api/v3/registration5-semver1/{group.Key}/index.json",
                Count = group.Count(),
                Lower = group.Min(pkg => $"{pkg.MajorVersion}.{pkg.MinorVersion}.{pkg.PatchVersion}"),
                Upper = group.Max(pkg => $"{pkg.MajorVersion}.{pkg.MinorVersion}.{pkg.PatchVersion}"),
                Parent = $"{baseUrl}/api/v3/registration5-semver1/",
                Items = group.Select(package => new Crany.Shared.Models.PackageVersionItem
                {
                    Id =
                        $"{baseUrl}/api/v3/registration5-semver1/{package.Name.ToLower()}/{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}.json",
                    CatalogEntry = new Crany.Shared.Models.CatalogEntry
                    {
                        Id =
                            $"{baseUrl}/api/v3/registration5-semver1/{package.Name.ToLower()}/{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}.json",
                        PackageId = package.Name,
                        PackageContent =
                            $"{baseUrl}/api/v3-flatcontainer/{package.Name.ToLower()}/{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}/{package.Name.ToLower()}.{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}.nupkg",
                        Version = $"{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}",
                        Description = package.Description,
                        Authors = package.Authors,
                        Tags = packageTags
                            .Where(pt => pt.PackageId == package.Id)
                            .Join(tags, pt => pt.TagId, t => t.Id, (pt, t) => t.Name)
                            .ToList(),
                        Dependencies = dependencies
                            .Where(d => d.PackageId == package.Id)
                            .Select(d => new Crany.Shared.Models.Dependency
                            {
                                Id = d.DependencyName,
                                VersionRange = $"[{d.MajorVersion}.{d.MinorVersion}.{d.PatchVersion}, )"
                            }).ToList(),
                        Published = package.CreatedDate.ToUniversalTime(),
                        LicenseUrl = "https://opensource.org/licenses/MIT"
                    }
                }).ToList()
            }).ToList();

        // Root nesnesini oluştur
        var packageVersionRoot = new Crany.Shared.Models.RegistrationIndex.PackageVersionRoot
        {
            Id = $"{baseUrl}/api/v3/registration5-semver1/",
            CommitId = Guid.NewGuid().ToString(),
            CommitTimeStamp = DateTime.UtcNow,
            Count = packageItems.Count,
            Items = packageItems
        };

        return Ok(packageVersionRoot);
    }

    [HttpGet("registration5-semver1/{id}/index.json")]
    public async Task<IActionResult> GetRegistrationPage(string id)
    {
        var normalizedId = id.ToLower();

        // Paket sürümlerini getir
        var packages = await context.Packages
            .Where(p => p.Name.ToLower() == normalizedId)
            .OrderBy(p => p.MajorVersion)
            .ThenBy(p => p.MinorVersion)
            .ThenBy(p => p.PatchVersion)
            .ToListAsync();

        if (!packages.Any())
        {
            return NotFound(new { Message = $"Package '{id}' not found." });
        }

        // Temel URL
        var baseUrl = $"{Request.Scheme}://{Request.Host}/api/v3/registration5-semver1/{normalizedId}";

        // `lower` ve `upper` sürüm bilgilerini belirle
        var lowerVersion =
            $"{packages.First().MajorVersion}.{packages.First().MinorVersion}.{packages.First().PatchVersion}";
        var upperVersion =
            $"{packages.Last().MajorVersion}.{packages.Last().MinorVersion}.{packages.Last().PatchVersion}";

        // Sürüm bilgilerini oluştur
        var items = packages.Select(p => new
        {
            Id = $"{baseUrl}/{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}.json",
            CatalogEntry = new
            {
                Id = $"{baseUrl}/{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}.json",
                PackageId = p.Name,
                Version = $"{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}",
                Description = p.Description,
                Authors = p.Authors,
                Tags = string.IsNullOrWhiteSpace(p.Tags)
                    ? new List<string>()
                    : p.Tags.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
                Dependencies = context.PackageDependencies
                    .Where(d => d.PackageId == p.Id)
                    .Select(d => new
                    {
                        Id = d.DependencyName,
                        VersionRange = $"[{d.MajorVersion}.{d.MinorVersion}.{d.PatchVersion}, )"
                    })
                    .ToList(),
                Published = p.CreatedDate.ToUniversalTime(),
                LicenseUrl = "https://opensource.org/licenses/MIT",
                PackageContent =
                    $"{Request.Scheme}://{Request.Host}/api/v3-flatcontainer/{normalizedId}/{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}/{normalizedId}.{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}.nupkg"
            }
        }).ToList();

        // Yanıt oluştur
        var response = new
        {
            Id = $"{baseUrl}/index.json",
            Count = items.Count,
            Lower = lowerVersion,
            Upper = upperVersion,
            Parent = $"{Request.Scheme}://{Request.Host}/api/v3/registration5-semver1/",
            Items = items
        };

        return Ok(response);
    }

    [HttpGet("v3/registration5-semver1/{id}/{version}.json")]
    public async Task<IActionResult> GetRegistrationLeaf(string id, string version)
    {
        var normalizedId = id.ToLower();
        var normalizedVersion = version.ToLower();

        if (!TryParseVersion(normalizedVersion, out var majorVersion, out var minorVersion, out var patchVersion))
        {
            return BadRequest(new { Message = "Invalid version format. Expected format: 'major.minor.patch'." });
        }

        // İlgili paketi ve sürümü al
        var package = await context.Packages
            .Where(p => p.Name.ToLower() == normalizedId &&
                        p.MajorVersion == majorVersion &&
                        p.MinorVersion == minorVersion &&
                        p.PatchVersion == patchVersion
            )
            .FirstOrDefaultAsync();

        if (package == null)
        {
            return NotFound(new { Message = $"Package '{id}' version '{version}' not found." });
        }

        // Dependencies'leri getir
        var dependencies = await context.PackageDependencies
            .Where(d => d.PackageId == package.Id)
            .Select(d => new Dependency()
            {
                Id = d.DependencyName,
                VersionRange = $"[{d.MajorVersion}.{d.MinorVersion}.{d.PatchVersion}, )"
            })
            .ToListAsync();

        // Response JSON
        var baseUrl = $"{Request.Scheme}://{Request.Host}/api/v3";
        var response = new Crany.Shared.Models.RegistrationLeaf.PackageVersionRoot
        {
            Id = $"{baseUrl}/registration5-semver1/{normalizedId}/{normalizedVersion}.json",
            CatalogEntry = new CatalogEntry
            {
                Id = $"{baseUrl}/registration5-semver1/{normalizedId}/{normalizedVersion}.json",
                PackageId = package.Name,
                Version = $"{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}",
                Authors = package.Authors,
                Description = package.Description,
                Tags = string.IsNullOrWhiteSpace(package.Tags)
                    ? []
                    : package.Tags.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
                Dependencies = dependencies,
                Published = package.CreatedDate.ToUniversalTime(),
                LicenseUrl = "https://opensource.org/licenses/MIT"
            },
            PackageContent =
                $"{baseUrl}-flatcontainer/{normalizedId}/{normalizedVersion}/{normalizedId}.{normalizedVersion}.nupkg",
            Registration = $"{baseUrl}/registration5-semver1/{normalizedId}/index.json"
        };

        return Ok(response);
    }

    [HttpGet("v3/registration5-gz-semver2/{id}/index.json")]
    public async Task<IActionResult> GetPackageRegistration(string id)
    {
        // Normalize ID for case-insensitivity
        var normalizedId = id.ToLower();

        // Fetch all versions of the specified package
        var packages = await context.Packages
            .Where(p => p.Name.ToLower() == normalizedId)
            .OrderBy(p => p.MajorVersion)
            .ThenBy(p => p.MinorVersion)
            .ThenBy(p => p.PatchVersion)
            .ToListAsync();

        if (!packages.Any())
        {
            return NotFound(new { Message = $"Package '{id}' not found." });
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}/api/v3/registration5-gz-semver2/{normalizedId}";

        // Create PackageVersionGroups
        var packageVersionGroups = packages.GroupBy(p => $"{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}")
            .Select(group => new Crany.Shared.Models.RegistrationIndex.PackageVersionGroup
            {
                Id = $"{baseUrl}/{group.Key}.json",
                Count = group.Count(),
                Lower = group.Key,
                Upper = group.Key,
                Parent = $"{baseUrl}/index.json",
                Items = group.Select(package => new Crany.Shared.Models.PackageVersionItem
                {
                    Id = $"{baseUrl}/{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}.json",
                    CatalogEntry = new Crany.Shared.Models.CatalogEntry
                    {
                        Id = $"{baseUrl}/{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}.json",
                        PackageId = package.Name,
                        Version = $"{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}",
                        Description = package.Description,
                        Authors = package.Authors,
                        Tags = string.IsNullOrWhiteSpace(package.Tags)
                            ? new List<string>()
                            : package.Tags.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList(),
                        Dependencies = context.PackageDependencies
                            .Where(d => d.PackageId == package.Id)
                            .Select(d => new Crany.Shared.Models.Dependency
                            {
                                Id = d.DependencyName,
                                VersionRange = $"[{d.MajorVersion}.{d.MinorVersion}.{d.PatchVersion}, )"
                            }).ToList(),
                        Published = package.CreatedDate.ToUniversalTime(),
                        LicenseUrl = "https://opensource.org/licenses/MIT",
                        PackageContent =
                            $"{Request.Scheme}://{Request.Host}/api/v3-flatcontainer/{normalizedId}/{group.Key}/{normalizedId}.{group.Key}.nupkg"
                    }
                }).ToList()
            }).ToList();

        // Create PackageVersionRoot
        var packageVersionRoot = new Crany.Shared.Models.RegistrationIndex.PackageVersionRoot
        {
            Id = $"{baseUrl}/index.json",
            CommitId = Guid.NewGuid().ToString(),
            CommitTimeStamp = DateTime.UtcNow,
            Count = packageVersionGroups.Count,
            Items = packageVersionGroups
        };

        return Ok(packageVersionRoot);
    }

    [HttpGet("query")]
    public async Task<IActionResult> SearchPackages([FromQuery] string q)
    {
        var query = q?.ToLower() ?? string.Empty;

        var packages = await context.Packages
            .Where(p => p.Name.ToLower().Contains(query) ||
                        p.Description.ToLower().Contains(query) ||
                        p.Tags.ToLower().Contains(query))
            .ToListAsync();

        if (!packages.Any())
        {
            return NotFound(new { Message = "No packages found matching the query." });
        }

        var response = new
        {
            totalHits = packages.Count,
            data = packages.Select(p => new
            {
                id = p.Name,
                version = $"{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}",
                description = p.Description,
                authors = p.Authors,
                tags = string.IsNullOrWhiteSpace(p.Tags)
                    ? []
                    : p.Tags.Split(", ", StringSplitOptions.RemoveEmptyEntries),
                versions = context.Packages
                    .Where(v => v.Name == p.Name)
                    .OrderBy(v => v.MajorVersion)
                    .ThenBy(v => v.MinorVersion)
                    .ThenBy(v => v.PatchVersion)
                    .Select(v => new
                    {
                        version = $"{v.MajorVersion}.{v.MinorVersion}.{v.PatchVersion}",
                        downloads = v.DownloadCount
                    })
            })
        };

        return Ok(response);
    }


    [HttpGet("v3-flatcontainer/{id}/index.json")]
    public async Task<IActionResult> GetPackageVersions(string id)
    {
        var normalizedId = id.ToLower();

        var packageVersions = await context.Packages
            .Where(p => p.Name.ToLower() == normalizedId)
            .OrderBy(p => p.MajorVersion)
            .ThenBy(p => p.MinorVersion)
            .ThenBy(p => p.PatchVersion)
            .Select(p => $"{p.MajorVersion}.{p.MinorVersion}.{p.PatchVersion}")
            .ToListAsync();

        if (packageVersions.Count == 0)
            return NotFound();

        Response.ContentType = "application/json";

        return Ok(new { Versions = packageVersions });
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