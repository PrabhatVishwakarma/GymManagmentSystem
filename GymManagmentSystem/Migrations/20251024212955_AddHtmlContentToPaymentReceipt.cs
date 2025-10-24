using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddHtmlContentToPaymentReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlContent",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HtmlContent",
                table: "PaymentReceipts");
        }
    }
}
