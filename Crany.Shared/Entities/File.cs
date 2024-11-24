using Crany.Shared.Enums;

namespace Crany.Shared.Entities;

public class File
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public string FileName { get; set; }
    public string? Content { get; set; } // Content of the file
    public string TargetPath { get; set; } // File path in the package
    public FileType Type { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public int? Weight { get; set; }
    public string Checksum { get; set; }
}