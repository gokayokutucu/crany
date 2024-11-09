using Crany.Web.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crany.Web.Api.Infrastructure.Configurations;

public class UserPackageConfiguration : IEntityTypeConfiguration<UserPackage>
{
    public void Configure(EntityTypeBuilder<UserPackage> builder)
    {
        builder.HasKey(up => up.Id);

        builder.HasOne(up => up.User)
            .WithMany(u => u.UserPackages)
            .HasForeignKey(up => up.UserId);

        builder.HasOne(up => up.Package)
            .WithMany(p => p.UserPackages)
            .HasForeignKey(up => up.PackageId);
    }
}