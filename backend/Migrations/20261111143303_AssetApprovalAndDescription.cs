using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class AssetApprovalAndDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing devices stay approved and active.
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Assets",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Assets",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Assets",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assets_ApprovalStatus",
                table: "Assets",
                column: "ApprovalStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Assets_ApprovalStatus", table: "Assets");
            migrationBuilder.DropColumn(name: "ApprovalStatus", table: "Assets");
            migrationBuilder.DropColumn(name: "Description", table: "Assets");
            migrationBuilder.DropColumn(name: "IsActive", table: "Assets");
        }
    }
}
