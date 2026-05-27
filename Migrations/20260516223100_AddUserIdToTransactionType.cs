using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantIA.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "TransactionTypes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_UserId",
                table: "TransactionTypes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionTypes_Users_UserId",
                table: "TransactionTypes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionTypes_Users_UserId",
                table: "TransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_TransactionTypes_UserId",
                table: "TransactionTypes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TransactionTypes");
        }
    }
}
