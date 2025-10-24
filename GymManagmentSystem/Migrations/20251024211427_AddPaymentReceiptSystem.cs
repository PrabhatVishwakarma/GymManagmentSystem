using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagmentSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceiptSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentReceipts",
                columns: table => new
                {
                    PaymentReceiptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MembersMembershipId = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MemberName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MemberEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MemberPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceipts", x => x.PaymentReceiptId);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_MembersMemberships_MembersMembershipId",
                        column: x => x.MembersMembershipId,
                        principalTable: "MembersMemberships",
                        principalColumn: "MembersMembershipId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_MembersMembershipId",
                table: "PaymentReceipts",
                column: "MembersMembershipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentReceipts");
        }
    }
}
