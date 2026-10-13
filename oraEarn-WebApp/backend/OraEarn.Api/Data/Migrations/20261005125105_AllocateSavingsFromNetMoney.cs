using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OraEarn.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllocateSavingsFromNetMoney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ManualAmount",
                table: "SavingsGoals",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ManualAmount",
                table: "SavingsGoals");
        }
    }
}
