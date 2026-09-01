using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmlKycProject.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Alerts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Alerts");
        }
    }
}
