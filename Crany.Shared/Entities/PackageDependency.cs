namespace Crany.Shared.Entities;

public class PackageDependency
{
    public int DependencyId { get; set; }
    public int PackageId { get; set; }
    
    public string DependencyName { get; set; }
    public int MajorVersion { get; set; }
    public int MinorVersion { get; set; }
    public int PatchVersion { get; set; }
    public string? PreReleaseTag { get; set; }
    public string? BuildMetadata { get; set; }
}