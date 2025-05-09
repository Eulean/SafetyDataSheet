using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDS.Migrations
{
    /// <inheritdoc />
    public partial class fixModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeaderHImages_SdsModels_SdsModelId",
                table: "HeaderHImages");

            migrationBuilder.DropIndex(
                name: "IX_HeaderHImages_SdsModelId",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "SdsModelId",
                table: "HeaderHImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SdsModelId",
                table: "HeaderHImages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderHImages_SdsModelId",
                table: "HeaderHImages",
                column: "SdsModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeaderHImages_SdsModels_SdsModelId",
                table: "HeaderHImages",
                column: "SdsModelId",
                principalTable: "SdsModels",
                principalColumn: "Id");
        }
    }
}
