using Crany.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crany.Web.Api.Infrastructure.Configurations;

public class PackageTagConfiguration : IEntityTypeConfiguration<PackageTag>
{
    public void Configure(EntityTypeBuilder<PackageTag> builder)
    {
        builder.HasKey(pt => new { pt.PackageId, pt.TagId });
    }
}