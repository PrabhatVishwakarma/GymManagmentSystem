using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityAndMembershipInactive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInactive",
                table: "MembersMemberships",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    ActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActivityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientContact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.ActivityId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropColumn(
                name: "IsInactive",
                table: "MembersMemberships");
        }
    }
}
