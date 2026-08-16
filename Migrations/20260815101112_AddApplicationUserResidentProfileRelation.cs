using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartSociety.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationUserResidentProfileRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "ResidentProfiles");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "ResidentProfiles");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "ResidentProfiles");

            migrationBuilder.DropColumn(
                name: "IsPrimaryResident",
                table: "ResidentProfiles");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "ResidentProfiles");

            migrationBuilder.RenameColumn(
                name: "MobileNumber",
                table: "ResidentProfiles",
                newName: "CNIC");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "ResidentProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "ResidentProfiles");

            migrationBuilder.RenameColumn(
                name: "CNIC",
                table: "ResidentProfiles",
                newName: "MobileNumber");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "ResidentProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "ResidentProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "ResidentProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryResident",
                table: "ResidentProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "ResidentProfiles",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
