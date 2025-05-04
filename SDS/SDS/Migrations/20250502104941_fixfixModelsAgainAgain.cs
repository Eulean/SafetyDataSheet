using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDS.Migrations
{
    /// <inheritdoc />
    public partial class fixfixModelsAgainAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeaderHImages_HeadersData_HeadersDataId",
                table: "HeaderHImages");

            migrationBuilder.DropForeignKey(
                name: "FK_HeaderHImages_HeadersH_HeadersHId",
                table: "HeaderHImages");

            migrationBuilder.DropIndex(
                name: "IX_HeaderHImages_HeadersDataId",
                table: "HeaderHImages");

            migrationBuilder.DropIndex(
                name: "IX_HeaderHImages_HeadersHId",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "HeaderHId",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "HeadersDataId",
                table: "HeaderHImages");

            migrationBuilder.DropColumn(
                name: "HeadersHId",
                table: "HeaderHImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeaderHId",
                table: "HeaderHImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeadersDataId",
                table: "HeaderHImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeadersHId",
                table: "HeaderHImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HeaderHImages_HeadersDataId",
                table: "HeaderHImages",
                column: "HeadersDataId");

            migrationBuilder.CreateIndex(
                name: "IX_HeaderHImages_HeadersHId",
                table: "HeaderHImages",
                column: "HeadersHId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeaderHImages_HeadersData_HeadersDataId",
                table: "HeaderHImages",
                column: "HeadersDataId",
                principalTable: "HeadersData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeaderHImages_HeadersH_HeadersHId",
                table: "HeaderHImages",
                column: "HeadersHId",
                principalTable: "HeadersH",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
