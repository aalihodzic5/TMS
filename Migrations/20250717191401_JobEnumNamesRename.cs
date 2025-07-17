using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMS.Migrations
{
    /// <inheritdoc />
    public partial class JobEnumNamesRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "loadType",
                table: "Job",
                newName: "LoadType");

            migrationBuilder.RenameColumn(
                name: "trailerType",
                table: "Job",
                newName: "TrailerTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LoadType",
                table: "Job",
                newName: "loadType");

            migrationBuilder.RenameColumn(
                name: "TrailerTypes",
                table: "Job",
                newName: "trailerType");
        }
    }
}
