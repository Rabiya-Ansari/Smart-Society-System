using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSociety.Migrations
{
    public partial class AddComplaintWorkTracking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "WorkNotes", table: "Complaints", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "SlaTargetDate", table: "Complaints", type: "datetime2", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WorkNotes", table: "Complaints");
            migrationBuilder.DropColumn(name: "SlaTargetDate", table: "Complaints");
        }
    }
}
