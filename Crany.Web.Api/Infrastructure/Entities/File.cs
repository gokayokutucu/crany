using Crany.Web.Api.Infrastructure.Entities.Enums;

namespace Crany.Web.Api.Infrastructure.Entities;

public class File
{
    public int Id { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
    public string FileName { get; set; }
    public string Content { get; set; } // Dosyanın içeriği
    public string TargetPath { get; set; } // Dosya dizini
    public FileType Type { get; set; }
    public string? Description { get; set; }
    public string? Title { get; set; }
    public int? Weight { get; set; }
}