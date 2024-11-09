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
            .HasMaxLength(100);

        builder.Property(pd => pd.PreReleaseTag)
            .HasMaxLength(20);

        builder.Property(pd => pd.BuildMetadata)
            .HasMaxLength(50);

        builder.HasOne(pd => pd.Package)
            .WithMany(p => p.Dependencies)
            .HasForeignKey(pd => pd.PackageId);
    }
}