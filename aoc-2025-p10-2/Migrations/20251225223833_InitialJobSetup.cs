using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace aoc_2025_p10_2.Migrations
{
    /// <inheritdoc />
    public partial class InitialJobSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MachineJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MachineNumber = table.Column<int>(type: "integer", nullable: false),
                    TargetData = table.Column<string>(type: "text", nullable: false),
                    ButtonsData = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDurationSeconds = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WorkerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MachineJobId = table.Column<int>(type: "integer", nullable: false),
                    ButtonPresses = table.Column<long>(type: "bigint", nullable: false),
                    Found = table.Column<bool>(type: "boolean", nullable: false),
                    ActualDurationMilliseconds = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobResults_MachineJobs_MachineJobId",
                        column: x => x.MachineJobId,
                        principalTable: "MachineJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobResults_MachineJobId",
                table: "JobResults",
                column: "MachineJobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MachineJobs_EstimatedDurationSeconds",
                table: "MachineJobs",
                column: "EstimatedDurationSeconds");

            migrationBuilder.CreateIndex(
                name: "IX_MachineJobs_Status",
                table: "MachineJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobResults");

            migrationBuilder.DropTable(
                name: "MachineJobs");
        }
    }
}
