using Crany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crany.Web.Api.Infrastructure.Configurations;
public class UserPackageConfiguration : IEntityTypeConfiguration<UserPackage>
{
    public void Configure(EntityTypeBuilder<UserPackage> builder)
    {
        builder.HasKey(up => up.Id);

        builder.Property(up => up.UserId)
            .IsRequired()
            .HasMaxLength(36);

        builder.Property(up => up.IsOwner).IsRequired();
        
        builder.Ignore(up => up.Package);
    }
}