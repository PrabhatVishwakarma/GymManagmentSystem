using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintsAndFixMembershipStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_Email",
                table: "Enquiries",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enquiries_Phone",
                table: "Enquiries",
                column: "Phone",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Enquiries_Email",
                table: "Enquiries");

            migrationBuilder.DropIndex(
                name: "IX_Enquiries_Phone",
                table: "Enquiries");
        }
    }
}
