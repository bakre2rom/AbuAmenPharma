using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbuAmenPharma.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_PurchaseLine_BatchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "PurchaseLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLines_BatchId",
                table: "PurchaseLines",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseLines_ItemBatches_BatchId",
                table: "PurchaseLines",
                column: "BatchId",
                principalTable: "ItemBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseLines_ItemBatches_BatchId",
                table: "PurchaseLines");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseLines_BatchId",
                table: "PurchaseLines");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "PurchaseLines");
        }
    }
}
