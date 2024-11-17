
namespace Crany.Shared.Entities;

public class UserPackage
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int PackageId { get; set; }
    public bool IsOwner { get; set; }
}