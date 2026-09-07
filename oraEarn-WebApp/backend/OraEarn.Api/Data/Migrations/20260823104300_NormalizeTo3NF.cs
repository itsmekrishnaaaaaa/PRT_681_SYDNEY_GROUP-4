using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OraEarn.Api.Data.Migrations;

/// <summary>Applies 3NF: drop transitive UserId, add PaymentMethods lookup.</summary>
[Migration("20260823104300_NormalizeTo3NF")]
public partial class NormalizeTo3NF : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Schema updates now live in AddPendingModelChanges so this can run on a fresh database.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE "Shifts" ADD COLUMN IF NOT EXISTS "UserId" text;
            ALTER TABLE "Expenses" ADD COLUMN IF NOT EXISTS "UserId" text;
            ALTER TABLE "Expenses" ADD COLUMN IF NOT EXISTS "PaymentMethod" text;
            """);
    }
}
