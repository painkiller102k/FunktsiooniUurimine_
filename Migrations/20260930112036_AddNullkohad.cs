using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunktsiooniUurimine.Migrations
{
    /// <inheritdoc />
    public partial class AddNullkohad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nullkohad",
                table: "FunktsiooniUurimised",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nullkohad",
                table: "FunktsiooniUurimised");
        }
    }
}
