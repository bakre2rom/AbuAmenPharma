using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbuAmenPharma.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeSalesmanIdNullableInSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Salesmen_SalesmanId",
                table: "Sales");

            migrationBuilder.AlterColumn<int>(
                name: "SalesmanId",
                table: "Sales",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Salesmen_SalesmanId",
                table: "Sales",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Salesmen_SalesmanId",
                table: "Sales");

            migrationBuilder.AlterColumn<int>(
                name: "SalesmanId",
                table: "Sales",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Salesmen_SalesmanId",
                table: "Sales",
                column: "SalesmanId",
                principalTable: "Salesmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
