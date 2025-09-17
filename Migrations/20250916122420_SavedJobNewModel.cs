using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TMS.Migrations
{
    /// <inheritdoc />
    public partial class SavedJobNewModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trailer_Truck_TruckId",
                table: "Trailer");

            migrationBuilder.CreateTable(
                name: "SavedJob",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    savedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedJob_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedJob_Job_JobId",
                        column: x => x.JobId,
                        principalTable: "Job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedJob_JobId",
                table: "SavedJob",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedJob_UserId_JobId",
                table: "SavedJob",
                columns: new[] { "UserId", "JobId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Trailer_Truck_TruckId",
                table: "Trailer",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trailer_Truck_TruckId",
                table: "Trailer");

            migrationBuilder.DropTable(
                name: "SavedJob");

            migrationBuilder.AddForeignKey(
                name: "FK_Trailer_Truck_TruckId",
                table: "Trailer",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
