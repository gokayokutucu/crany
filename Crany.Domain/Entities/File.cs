using Crany.Domain.Entities.Enums;

namespace Crany.Domain.Entities;

public class File
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
    public string FileName { get; set; }
    public string Content { get; set; } // Content of the file
    public string TargetPath { get; set; } // File path in the package
    public FileType Type { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public int? Weight { get; set; }
}