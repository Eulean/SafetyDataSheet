using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDS.Migrations
{
    /// <inheritdoc />
    public partial class fixImageModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentID",
                table: "HeaderHImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductId",
                table: "HeaderHImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentID",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "HeaderHImages");
        }
    }
}
