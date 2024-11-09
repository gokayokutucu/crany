namespace Crany.Web.Api.Infrastructure.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<PackageTag> PackageTags { get; set; } = new List<PackageTag>();
}

public class PackageTag
{
    public int PackageId { get; set; }
    public Package Package { get; set; }

    public int TagId { get; set; }
    public Tag Tag { get; set; }
}