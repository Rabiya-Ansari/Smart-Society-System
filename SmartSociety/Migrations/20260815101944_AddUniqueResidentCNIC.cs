using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSociety.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueResidentCNIC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CNIC",
                table: "ResidentProfiles",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ResidentProfiles_CNIC",
                table: "ResidentProfiles",
                column: "CNIC",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ResidentProfiles_CNIC",
                table: "ResidentProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "CNIC",
                table: "ResidentProfiles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
