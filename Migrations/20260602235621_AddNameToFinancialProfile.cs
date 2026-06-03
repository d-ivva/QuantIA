using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantIA.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToFinancialProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "FinancialProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "FinancialProfiles");
        }
    }
}
