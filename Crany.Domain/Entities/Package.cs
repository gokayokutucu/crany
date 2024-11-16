namespace Crany.Domain.Entities;

public class Package
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    public int PatchVersion { get; set; }
    public string? PreReleaseTag { get; set; }
    public string? BuildMetadata { get; set; }
    public string? Description { get; set; }
    public string? Authors { get; set; }  // .nuspec Authors
    public string? ProjectUrl { get; set; }
    public string? LicenseUrl { get; set; }
    public string? IconUrl { get; set; }
    public string? Tags { get; set; }
    public string? Summary { get; set; }
    public string? ReleaseNotes { get; set; }
    public string? Copyright { get; set; }
    public bool RequireLicenseAcceptance { get; set; }
    public bool IsDevelopmentDependency { get; set; }
    public bool IsLegacy { get; set; }
    public bool HasCriticalBugs { get; set; }
    public string? AlternatePackage { get; set; }
    public string? AlternatePackageVersion { get; set; }
    public string? Readme { get; set; }
    public string? RepositoryType { get; set; }
    public string? RepositoryUrl { get; set; }
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int DownloadCount { get; set; } = 0; // Number of downloads
    public ICollection<PackageDependency> Dependencies { get; set; } = new List<PackageDependency>();
    public ICollection<File> Files { get; set; } = new List<File>();
    public ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
    public ICollection<PackageTag> PackageTags { get; set; } = new List<PackageTag>();
}