using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using File = Crany.Web.Api.Infrastructure.Entities.File;

namespace Crany.Web.Api.Infrastructure.Configurations;


public class FileConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(256);
        builder.Property(f => f.Content).IsRequired();
        builder.Property(f => f.TargetPath).IsRequired().HasMaxLength(512);
        builder.Property(f => f.Type).IsRequired().HasConversion<string>();
        builder.Property(f => f.Description).HasMaxLength(4000);
        builder.Property(f => f.Title).HasMaxLength(256);
    }
}