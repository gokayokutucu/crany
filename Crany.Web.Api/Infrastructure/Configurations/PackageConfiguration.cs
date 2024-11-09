using Crany.Web.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crany.Web.Api.Infrastructure.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PreReleaseTag)
            .HasMaxLength(20);

        builder.Property(p => p.BuildMetadata)
            .HasMaxLength(50);

        builder.Property(p => p.Description)
            .HasMaxLength(255);

        builder.Property(p => p.Tags)
            .HasMaxLength(255);

        builder.Property(p => p.AlternatePackage)
            .HasMaxLength(100);

        builder.Property(p => p.AlternatePackageVersion)
            .HasMaxLength(20);
    }
}