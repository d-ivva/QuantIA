using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantIA.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTypesSeeded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypesSeeded",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TypesSeeded",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
