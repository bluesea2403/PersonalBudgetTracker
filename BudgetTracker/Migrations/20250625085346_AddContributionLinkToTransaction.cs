using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddContributionLinkToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GoalContributionId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_GoalContributionId",
                table: "Transactions",
                column: "GoalContributionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_GoalContributions_GoalContributionId",
                table: "Transactions",
                column: "GoalContributionId",
                principalTable: "GoalContributions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_GoalContributions_GoalContributionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_GoalContributionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "GoalContributionId",
                table: "Transactions");
        }
    }
}
