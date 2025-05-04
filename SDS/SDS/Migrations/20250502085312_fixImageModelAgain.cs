using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDS.Migrations
{
    /// <inheritdoc />
    public partial class fixImageModelAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "HeaderHImages",
                newName: "ContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "HeaderHImages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "HeaderHImages");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "HeaderHImages",
                newName: "ImagePath");
        }
    }
}
