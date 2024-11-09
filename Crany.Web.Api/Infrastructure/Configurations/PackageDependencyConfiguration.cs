using Crany.Web.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crany.Web.Api.Infrastructure.Configurations;

public class PackageDependencyConfiguration : IEntityTypeConfiguration<PackageDependency>
{
    public void Configure(EntityTypeBuilder<PackageDependency> builder)
    {
        builder.HasKey(pd => pd.DependencyId);

        builder.Property(pd => pd.DependencyName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(pd => pd.MajorVersion).IsRequired();
        builder.Property(pd => pd.MinorVersion).IsRequired();
        builder.Property(pd => pd.PatchVersion).IsRequired();
        builder.Property(pd => pd.PreReleaseTag).HasMaxLength(20);
        builder.Property(pd => pd.BuildMetadata).HasMaxLength(20);
    }
}