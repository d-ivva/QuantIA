using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantIA.Migrations
{
    /// <inheritdoc />
    public partial class AddTypesSeededToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TypesSeeded",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypesSeeded",
                table: "Users");
        }
    }
}
