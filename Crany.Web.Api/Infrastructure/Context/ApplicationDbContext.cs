using Crany.Web.Api.Infrastructure.Configurations;
using Crany.Web.Api.Infrastructure.Entities;
using Crany.Web.Api.Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using File = Crany.Web.Api.Infrastructure.Entities.File;

namespace Crany.Web.Api.Infrastructure.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageDependency> PackageDependencies { get; set; }
    public DbSet<File> ProtoFiles { get; set; }
    public DbSet<UserPackage> UserPackages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PackageConfiguration());
        modelBuilder.ApplyConfiguration(new PackageDependencyConfiguration());
        modelBuilder.ApplyConfiguration(new FileConfiguration());
        modelBuilder.ApplyConfiguration(new UserPackageConfiguration());
    }
}