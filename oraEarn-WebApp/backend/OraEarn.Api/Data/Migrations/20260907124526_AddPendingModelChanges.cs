using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OraEarn.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_AspNetUsers_UserId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_AspNetUsers_UserId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_StartTime",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SavingsGoals");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "SavingsGoals");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Expenses");

            migrationBuilder.RenameColumn(
                name: "HourlyRate",
                table: "Shifts",
                newName: "TotalHours");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "SavingsGoals",
                newName: "Title");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "StartTime",
                table: "Shifts",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "EndTime",
                table: "Shifts",
                type: "time without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "Shifts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalEarned",
                table: "Shifts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "WorkDate",
                table: "Shifts",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AlterColumn<DateOnly>(
                name: "TargetDate",
                table: "SavingsGoals",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AllocationPercentage",
                table: "SavingsGoals",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "Expenses",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Expenses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ExpenseKind",
                table: "Expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethodId",
                table: "Expenses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AiReviewSummary",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdentityDocumentType",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisaDocumentFileName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisaDocumentPath",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "VisaExpiryDate",
                table: "AspNetUsers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisaStatus",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisaType",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkingRights",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseCategories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DateOfJoin = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationReads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    NotificationKey = table.Column<string>(type: "text", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationReads_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveShiftSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    BreakMinutesAccumulated = table.Column<int>(type: "integer", nullable: false),
                    BreakStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveShiftSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveShiftSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveShiftSessions_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_JobId",
                table: "Shifts",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_WorkDate",
                table: "Shifts",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaymentMethodId",
                table: "Expenses",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveShiftSessions_JobId",
                table: "ActiveShiftSessions",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveShiftSessions_UserId",
                table: "ActiveShiftSessions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_UserId",
                table: "ExpenseCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_UserId",
                table: "Jobs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_Name",
                table: "PaymentMethods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationReads_UserId_NotificationKey",
                table: "UserNotificationReads",
                columns: new[] { "UserId", "NotificationKey" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseCategories_CategoryId",
                table: "Expenses",
                column: "CategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_PaymentMethods_PaymentMethodId",
                table: "Expenses",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Jobs_JobId",
                table: "Shifts",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseCategories_CategoryId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_PaymentMethods_PaymentMethodId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Jobs_JobId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "ActiveShiftSessions");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "UserNotificationReads");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_JobId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_WorkDate",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_CategoryId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_PaymentMethodId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "TotalEarned",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "WorkDate",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "AllocationPercentage",
                table: "SavingsGoals");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ExpenseKind",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "AiReviewSummary",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdentityDocumentType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VisaDocumentFileName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VisaDocumentPath",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VisaExpiryDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VisaStatus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VisaType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkingRights",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "TotalHours",
                table: "Shifts",
                newName: "HourlyRate");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "SavingsGoals",
                newName: "Name");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Shifts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Shifts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(TimeOnly),
                oldType: "time without time zone");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Shifts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Shifts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Shifts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TargetDate",
                table: "SavingsGoals",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SavingsGoals",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "SavingsGoals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Expenses",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Expenses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Expenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Expenses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Expenses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StartTime",
                table: "Shifts",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_UserId",
                table: "Shifts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_UserId",
                table: "Expenses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_AspNetUsers_UserId",
                table: "Expenses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_AspNetUsers_UserId",
                table: "Shifts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
