using Crany.Web.Api.Infrastructure.Entities;
using File = Crany.Web.Api.Infrastructure.Entities.File;

namespace Crany.Web.Api.Infrastructure.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProtoFileConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        builder.HasKey(pf => pf.Id);

        builder.Property(pf => pf.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(pf => pf.TargetPath)
            .HasMaxLength(255);

        builder.Property(pf => pf.Title)
            .HasMaxLength(255);

        builder.Property(pf => pf.Description)
            .HasMaxLength(255);

        builder.Property(pf => pf.Type)
            .HasMaxLength(50);

        builder.HasOne(pf => pf.Package)
            .WithMany(p => p.Files)
            .HasForeignKey(pf => pf.PackageId);
    }
}