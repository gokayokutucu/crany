using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crany.Web.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RepositoryTypeurlAndVisibilityToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Packages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryType",
                table: "Packages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepositoryUrl",
                table: "Packages",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "RepositoryType",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "RepositoryUrl",
                table: "Packages");
        }
    }
}
