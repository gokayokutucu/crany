using Crany.Web.Api.Infrastructure.Entities.Identity;

namespace Crany.Web.Api.Infrastructure.Entities;

public class UserPackage
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int PackageId { get; set; }
    public Package Package { get; set; }
    public bool IsOwner { get; set; }
}