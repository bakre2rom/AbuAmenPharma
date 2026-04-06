using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbuAmenPharma.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalizedNameConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NameArNormalized",
                table: "Units",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Units]
                SET
                    [NameAr] = LTRIM(RTRIM([NameAr])),
                    [NameArNormalized] = NULLIF(UPPER(LTRIM(RTRIM([NameAr]))), '')
                WHERE [NameAr] IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                ;WITH DuplicateUnits AS (
                    SELECT
                        [Id],
                        ROW_NUMBER() OVER (PARTITION BY [NameArNormalized] ORDER BY [Id]) AS [Rn]
                    FROM [Units]
                    WHERE [IsActive] = 1 AND [NameArNormalized] IS NOT NULL
                )
                UPDATE [Units]
                SET [IsActive] = 0
                WHERE [Id] IN (
                    SELECT [Id]
                    FROM DuplicateUnits
                    WHERE [Rn] > 1
                );
                """);

            migrationBuilder.Sql("""
                UPDATE [Customers]
                SET
                    [Name] = LTRIM(RTRIM([Name])),
                    [NameNormalized] = NULLIF(UPPER(LTRIM(RTRIM([Name]))), '')
                WHERE [Name] IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                ;WITH DuplicateCustomers AS (
                    SELECT
                        [Id],
                        ROW_NUMBER() OVER (PARTITION BY [NameNormalized] ORDER BY [Id]) AS [Rn]
                    FROM [Customers]
                    WHERE [IsActive] = 1 AND [NameNormalized] IS NOT NULL
                )
                UPDATE [Customers]
                SET [IsActive] = 0
                WHERE [Id] IN (
                    SELECT [Id]
                    FROM DuplicateCustomers
                    WHERE [Rn] > 1
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Units_NameArNormalized_Active",
                table: "Units",
                column: "NameArNormalized",
                unique: true,
                filter: "[IsActive] = 1 AND [NameArNormalized] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NameNormalized_Active",
                table: "Customers",
                column: "NameNormalized",
                unique: true,
                filter: "[IsActive] = 1 AND [NameNormalized] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Units_NameArNormalized_Active",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Customers_NameNormalized_Active",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NameArNormalized",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Customers");
        }
    }
}
