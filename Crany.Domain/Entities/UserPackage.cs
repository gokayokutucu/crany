
namespace Crany.Domain.Entities;

public class UserPackage
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
    public bool IsOwner { get; set; }
}