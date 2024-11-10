using Crany.Domain.Entities;
using Crany.Web.Api.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using File = Crany.Domain.Entities.File;

namespace Crany.Web.Api.Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageDependency> PackageDependencies { get; set; }
    public DbSet<File> Files { get; set; }
    public DbSet<UserPackage> UserPackages { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PackageTag> PackageTags { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new PackageConfiguration());
        builder.ApplyConfiguration(new PackageDependencyConfiguration());
        builder.ApplyConfiguration(new FileConfiguration());
        builder.ApplyConfiguration(new UserPackageConfiguration());
        builder.ApplyConfiguration(new TagConfiguration());
        builder.ApplyConfiguration(new PackageTagConfiguration());
    }
}