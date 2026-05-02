using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantIA.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionTypeDirection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InstallmentGroups_Accounts_AccountId",
                table: "InstallmentGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_InstallmentGroups_Users_UserId",
                table: "InstallmentGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_InstallmentGroups_AccountId",
                table: "InstallmentGroups");

            migrationBuilder.DropIndex(
                name: "IX_InstallmentGroups_UserId",
                table: "InstallmentGroups");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.Sql("ALTER TABLE \"TransactionTypes\" ALTER COLUMN \"Direction\" TYPE integer USING (CASE WHEN \"Direction\"='income' THEN 0 WHEN \"Direction\"='expense' THEN 1 ELSE 0 END);");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Direction",
                table: "TransactionTypes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentGroups_AccountId",
                table: "InstallmentGroups",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentGroups_UserId",
                table: "InstallmentGroups",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_InstallmentGroups_Accounts_AccountId",
                table: "InstallmentGroups",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InstallmentGroups_Users_UserId",
                table: "InstallmentGroups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
