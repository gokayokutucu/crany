using Crany.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crany.Web.Api.Infrastructure.Configurations;


public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(256);
        builder.Property(p => p.Description).HasMaxLength(4000);
        builder.Property(p => p.Authors).HasMaxLength(256);
        builder.Property(p => p.ProjectUrl).HasMaxLength(4000);
        builder.Property(p => p.LicenseUrl).HasMaxLength(4000);
        builder.Property(p => p.IconUrl).HasMaxLength(4000);
        builder.Property(p => p.Tags).HasMaxLength(4000);
        builder.Property(p => p.Summary).HasMaxLength(4000);
        builder.Property(p => p.ReleaseNotes).HasMaxLength(35000);
        builder.Property(p => p.Copyright).HasMaxLength(4000);
        builder.Property(p => p.RequireLicenseAcceptance).HasDefaultValue(false);
        builder.Property(p => p.IsDevelopmentDependency).HasDefaultValue(false);
        builder.Property(p => p.IsLegacy).HasDefaultValue(false);
        builder.Property(p => p.HasCriticalBugs).HasDefaultValue(false);
        builder.Property(p => p.DownloadCount).HasDefaultValue(0);
        builder.Property(p => p.CreatedDate).HasDefaultValueSql("NOW()");
    }
}