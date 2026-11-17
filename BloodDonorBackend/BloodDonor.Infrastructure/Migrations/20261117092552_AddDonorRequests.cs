using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodDonor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDonorRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonorRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BloodRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonorProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorRequests_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonorRequests_BloodRequests_BloodRequestId",
                        column: x => x.BloodRequestId,
                        principalTable: "BloodRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonorRequests_DonorProfiles_DonorProfileId",
                        column: x => x.DonorProfileId,
                        principalTable: "DonorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonorContactAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonorRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonorContactAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonorContactAccessLogs_AspNetUsers_AccessedByUserId",
                        column: x => x.AccessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonorContactAccessLogs_DonorRequests_DonorRequestId",
                        column: x => x.DonorRequestId,
                        principalTable: "DonorRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DonorContactAccessLogs_AccessedByUserId",
                table: "DonorContactAccessLogs",
                column: "AccessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorContactAccessLogs_DonorRequestId",
                table: "DonorContactAccessLogs",
                column: "DonorRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorRequests_BloodRequestId_DonorProfileId",
                table: "DonorRequests",
                columns: new[] { "BloodRequestId", "DonorProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonorRequests_BloodRequestId_Status",
                table: "DonorRequests",
                columns: new[] { "BloodRequestId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DonorRequests_CreatedByUserId",
                table: "DonorRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DonorRequests_DonorProfileId_Status",
                table: "DonorRequests",
                columns: new[] { "DonorProfileId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonorContactAccessLogs");

            migrationBuilder.DropTable(
                name: "DonorRequests");
        }
    }
}
