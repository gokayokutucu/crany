using System.IO.Compression;
using System.Xml.Linq;
using Crany.Shared.Entities;
using Crany.Shared.Enums;
using Crany.Shared.Helpers;
using Crany.Web.Api.Infrastructure.Context;
using Crany.Web.Api.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using static System.Int32;
using File = Crany.Shared.Entities.File;

namespace Crany.Web.Api.Services;

public class PushPackageService(ILogger<PushPackageService> logger, ApplicationDbContext context) : IPushPackageService
{
    public async Task<(bool Success, string Message)> PushPackageAsync(string? userUid, IFormFile? packageFile)
    {
        if (string.IsNullOrEmpty(userUid))
        {
            return (false, "No user provided.");
        }

        if (packageFile == null || packageFile.Length == 0)
        {
            return (false, "No package file provided.");
        }

        if (packageFile.Length > 1048576)
        {
            return (false, "File size exceeds the 1 MB limit.");
        }

        // Validate the file and get checksum
        var (isValid, checksum) = await ValidatePackageFile(packageFile);
        if (!isValid)
        {
            return (false, "Invalid or duplicate package file.");
        }

        // Save the package file to disk
        var savePath = SavePackageToDisk(packageFile);

        // Save Nupkg file path to the database
        await AddPackagePathToFiles(savePath, checksum);

        // Extract package metadata, proto files, and tags
        var (package, packageDependencies, protoFiles, tags) = ExtractPackageAndProtoFiles(savePath);
        if (package == null)
        {
            return (false, "Failed to extract package metadata or proto files.");
        }

        // Set checksum for the package
        package.Checksum = checksum;

        // Save all related entities to the database
        await SavePackageData(userUid, package, protoFiles, tags, packageDependencies);

        return (true, "Package pushed successfully.");
    }

    private async Task AddPackagePathToFiles(string savePath, string checksum)
    {
        // Ensure that the path and checksum are valid
        if (string.IsNullOrWhiteSpace(savePath) || string.IsNullOrWhiteSpace(checksum))
        {
            throw new ArgumentException("Invalid savePath or checksum");
        }

        var nupkgFile = await context.Files.FirstOrDefaultAsync(f => f.Checksum == checksum);

        if (nupkgFile is null)
        {
            return;
        }

        nupkgFile = new File
        {
            FileName = Path.GetFileName(savePath),
            TargetPath = savePath,
            Type = FileType.Nupkg,
            Checksum = checksum
        };

        context.Files.Add(nupkgFile);
    }

    private async Task<(bool IsValid, string Checksum)> ValidatePackageFile(IFormFile packageFile)
    {
        try
        {
            await using var stream = packageFile.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, true);

            // Calculate checksum
            stream.Position = 0;
            var checksum = Encryption.CalculateChecksum(stream);

            // Check for duplicate packages
            var isDuplicate = await context.Packages.AnyAsync(p => p.Checksum == checksum);
            return (!isDuplicate, checksum);
        }
        catch
        {
            return (false, string.Empty);
        }
    }

    private string SavePackageToDisk(IFormFile packageFile)
    {
        var packageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "PackageStorage");
        Directory.CreateDirectory(packageDirectory);

        var savePath = Path.Combine(packageDirectory, packageFile.FileName);
        using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        packageFile.CopyTo(fileStream);

        return savePath;
    }

    private async Task SavePackageData(string userUid, Package package, List<File> protoFiles, List<Tag> tags,
        List<PackageDependency> dependencies)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // Save the package to the database to generate its ID
            context.Packages.Add(package);
            await context.SaveChangesAsync(); // Ensure `package.Id` is generated

            // Add tags and associate them with the package
            foreach (var tag in tags)
            {
                // Check if the tag already exists in the database
                var existingTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == tag.Name);
                var tagId = existingTag?.Id ?? 0;

                if (existingTag == null)
                {
                    context.Tags.Add(tag);
                    await context.SaveChangesAsync(); // Ensure `tag.Id` is generated
                    tagId = tag.Id;
                }

                // Add the association between the package and the tag
                context.PackageTags.Add(new PackageTag
                {
                    PackageId = package.Id,
                    TagId = tagId
                });
            }

            // Add package dependencies if any
            if (dependencies.Count > 0)
            {
                foreach (var dependency in dependencies)
                {
                    context.PackageDependencies.Add(new PackageDependency
                    {
                        DependencyName = dependency.DependencyName,
                        MajorVersion = dependency.MajorVersion,
                        MinorVersion = dependency.MinorVersion,
                        PatchVersion = dependency.PatchVersion,
                        PreReleaseTag = dependency.PreReleaseTag,
                        BuildMetadata = dependency.BuildMetadata,
                        PackageId = package.Id // Associate with the package
                    });
                }
            }

            // Associate the package with the user
            context.UserPackages.Add(new UserPackage
            {
                UserId = userUid,
                PackageId = package.Id,
                IsOwner = true // Assuming the uploader is the owner
            });

            // Save proto files and associate them with the package
            foreach (var protoFile in protoFiles)
            {
                protoFile.PackageId = package.Id; // Associate file with the package
                protoFile.Checksum = Encryption.CalculateChecksum(protoFile.Content);
                context.Files.Add(protoFile);
            }

            // Save all changes to the database
            await context.SaveChangesAsync();
            
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Error while saving package data", ex);
        }
    }

    private (Package? package, List<PackageDependency> packageDependencies, List<File> protoFiles, List<Tag> tags)
        ExtractPackageAndProtoFiles(
            string packageFilePath)
    {
        try
        {
            using var fileStream = new FileStream(packageFilePath, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

            var nuspecEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".nuspec"));
            if (nuspecEntry == null) return (null, [], [], []);

            using var nuspecStream = nuspecEntry.Open();
            using var reader = new StreamReader(nuspecStream);
            var nuspecContent = reader.ReadToEnd();

            return ParseNuspecContent(nuspecContent, archive);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting nupkg: {Message}", ex.Message);
            return (null, [], [], []);
        }
    }

    private (Package? package, List<PackageDependency> packageDependencies, List<File> protoFiles, List<Tag> tags)
        ParseNuspecContent(string nuspecContent,
            ZipArchive archive)
    {
        var xmlDoc = XDocument.Parse(nuspecContent);

        // Define the namespace dynamically
        var ns = xmlDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

        // Extract metadata information
        var metadata = xmlDoc.Element(ns + "package")?.Element(ns + "metadata");
        if (metadata == null) return (null, [], [], []);

        var packageName = metadata.Element(ns + "id")?.Value;
        var version = metadata.Element(ns + "version")?.Value;
        var description = metadata.Element(ns + "description")?.Value;

        if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(version))
            return (null, [], [], []);

        var versionParts = version.Split('.');
        _ = TryParse(versionParts[0], out var majorVersion);
        _ = TryParse(versionParts[1], out var minorVersion);
        _ = TryParse(versionParts[2], out var patchVersion);

        var package = new Package
        {
            Name = packageName,
            MajorVersion = majorVersion,
            MinorVersion = minorVersion,
            PatchVersion = patchVersion,
            Description = description,
            CreatedDate = DateTime.UtcNow
        };

        // Parse dependencies
        var dependencies = ExtractPackageDependencies(metadata, ns);

        // Parse tags
        var tags = ExtractTags(nuspecContent);

        // Parse `.proto` files
        var protoFiles = ExtractProtoFiles(archive, package.Id);

        return (package, dependencies, protoFiles, tags);
    }

    private static List<PackageDependency> ExtractPackageDependencies(XElement metadata, XNamespace ns)
    {
        return metadata.Element(ns + "dependencies")?.Elements(ns + "dependency")
            .Select(d => new PackageDependency
            {
                DependencyName = d.Attribute("id")?.Value ??
                                 throw new InvalidDataException("Dependency name is missing"),
                MajorVersion = Parse(d.Attribute("version")?.Value.Split('.')[0] ?? "0"),
                MinorVersion = Parse(d.Attribute("version")?.Value.Split('.')[1] ?? "0"),
                PatchVersion = Parse(d.Attribute("version")?.Value.Split('.')[2] ?? "0")
            })
            .ToList() ?? [];
    }

    private static List<File> ExtractProtoFiles(ZipArchive archive, int packageId)
    {
        var protoFiles = new List<File>();
        foreach (var entry in archive.Entries.Where(e => e.FullName.EndsWith(".proto")))
        {
            using var protoStream = entry.Open();
            using var protoReader = new StreamReader(protoStream);
            var content = protoReader.ReadToEnd();

            protoFiles.Add(new File
            {
                FileName = entry.Name,
                Content = content,
                TargetPath = entry.FullName,
                PackageId = packageId
            });
        }

        return protoFiles;
    }

    private List<Tag> ExtractTags(string xmlContent)
    {
        try
        {
            var xmlDoc = XDocument.Parse(xmlContent);

            var ns = xmlDoc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            var tagsElement = xmlDoc.Element(ns + "package")?.Element(ns + "metadata")?.Element(ns + "tags");
            if (tagsElement == null || string.IsNullOrWhiteSpace(tagsElement.Value))
                return [];

            return tagsElement.Value
                .Split([' '], StringSplitOptions.RemoveEmptyEntries)
                .Select(tag => new Tag { Name = tag.Trim() })
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting tags: {Message}", ex.Message);
            return [];
        }
    }
}