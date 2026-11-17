using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodDonor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDonorAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForRequests",
                table: "DonorProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailableForRequests",
                table: "DonorProfiles");
        }
    }
}
