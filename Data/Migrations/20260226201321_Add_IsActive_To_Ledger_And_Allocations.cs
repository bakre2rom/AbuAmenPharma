using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbuAmenPharma.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_IsActive_To_Ledger_And_Allocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomerReceiptAllocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CustomerLedgers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomerReceiptAllocations");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CustomerLedgers");
        }
    }
}
