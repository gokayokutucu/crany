namespace Crany.Shared.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class PackageTag
{
    public int PackageId { get; set; }
    public int TagId { get; set; }
}