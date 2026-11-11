using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class InspectionsAndSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InspectionIntervalMonths",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NextInspectionDate",
                table: "Assets",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inspections_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Inspections_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InspectionAddresses",
                columns: table => new
                {
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    Street = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    BuildingNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ApartmentNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    PostalCode = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    City = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionAddresses", x => x.InspectionId);
                    table.ForeignKey(
                        name: "FK_InspectionAddresses_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InspectionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    InspectionId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PerformedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    NextInspectionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true),
                    CreatedById = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReviewedById = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InspectionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InspectionRecords_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InspectionRecords_AspNetUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InspectionRecords_Inspections_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "Inspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_NextInspectionDate",
                table: "Assets",
                column: "NextInspectionDate");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRecords_CreatedById",
                table: "InspectionRecords",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRecords_InspectionId",
                table: "InspectionRecords",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRecords_PerformedAt",
                table: "InspectionRecords",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRecords_ReviewedById",
                table: "InspectionRecords",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_InspectionRecords_Status",
                table: "InspectionRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_AssetId",
                table: "Inspections",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_OwnerId",
                table: "Inspections",
                column: "OwnerId");

            // Existing devices: first inspection a year from now, and an inspection card each.
            migrationBuilder.Sql("UPDATE `Assets` SET `NextInspectionDate` = DATE_ADD(CURDATE(), INTERVAL 12 MONTH);");
            migrationBuilder.Sql("INSERT INTO `Inspections` (`AssetId`, `OwnerId`) SELECT `Id`, `OwnerId` FROM `Assets`;");
            migrationBuilder.Sql(
                "INSERT INTO `InspectionAddresses` (`InspectionId`, `Street`, `BuildingNumber`, `ApartmentNumber`, `PostalCode`, `City`, `Country`) " +
                "SELECT i.`Id`, a.`Street`, a.`BuildingNumber`, a.`ApartmentNumber`, a.`PostalCode`, a.`City`, a.`Country` " +
                "FROM `AssetAddresses` a JOIN `Inspections` i ON i.`AssetId` = a.`AssetId`;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InspectionAddresses");

            migrationBuilder.DropTable(
                name: "InspectionRecords");

            migrationBuilder.DropTable(
                name: "Inspections");

            migrationBuilder.DropIndex(
                name: "IX_Assets_NextInspectionDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "InspectionIntervalMonths",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "NextInspectionDate",
                table: "Assets");
        }
    }
}
