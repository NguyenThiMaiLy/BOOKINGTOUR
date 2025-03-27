using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingTour.Migrations
{
    /// <inheritdoc />
    public partial class idinvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<long>(
                name: "IdInvoice",
                table: "BOOKING",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InvoiceNavigationIdInvoice",
                table: "BOOKING",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BOOKING_InvoiceNavigationIdInvoice",
                table: "BOOKING",
                column: "InvoiceNavigationIdInvoice");

            migrationBuilder.AddForeignKey(
                name: "FK_BOOKING_INVOICE_InvoiceNavigationIdInvoice",
                table: "BOOKING",
                column: "InvoiceNavigationIdInvoice",
                principalTable: "INVOICE",
                principalColumn: "ID_INVOICE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
