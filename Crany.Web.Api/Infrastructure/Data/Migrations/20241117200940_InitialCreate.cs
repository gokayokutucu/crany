using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crany.Web.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TargetPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    Checksum = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageDependencies",
                columns: table => new
                {
                    DependencyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    DependencyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MajorVersion = table.Column<int>(type: "integer", nullable: false),
                    MinorVersion = table.Column<int>(type: "integer", nullable: false),
                    PatchVersion = table.Column<int>(type: "integer", nullable: false),
                    PreReleaseTag = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    BuildMetadata = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDependencies", x => x.DependencyId);
                });

            migrationBuilder.CreateTable(
                name: "PackageTags",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTags", x => new { x.PackageId, x.TagId });
                });

            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MajorVersion = table.Column<int>(type: "integer", nullable: false),
                    MinorVersion = table.Column<int>(type: "integer", nullable: false),
                    PatchVersion = table.Column<int>(type: "integer", nullable: false),
                    PreReleaseTag = table.Column<string>(type: "text", nullable: true),
                    BuildMetadata = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Authors = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ProjectUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LicenseUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Tags = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReleaseNotes = table.Column<string>(type: "character varying(35000)", maxLength: 35000, nullable: true),
                    Copyright = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequireLicenseAcceptance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDevelopmentDependency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLegacy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HasCriticalBugs = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AlternatePackage = table.Column<string>(type: "text", nullable: true),
                    AlternatePackageVersion = table.Column<string>(type: "text", nullable: true),
                    Readme = table.Column<string>(type: "text", nullable: true),
                    RepositoryType = table.Column<string>(type: "text", nullable: true),
                    RepositoryUrl = table.Column<string>(type: "text", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    Checksum = table.Column<string>(type: "text", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    IsOwner = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPackages", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "PackageDependencies");

            migrationBuilder.DropTable(
                name: "PackageTags");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "UserPackages");
        }
    }
}
