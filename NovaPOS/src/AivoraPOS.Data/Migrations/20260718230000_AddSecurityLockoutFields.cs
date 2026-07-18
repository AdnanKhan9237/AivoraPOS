using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using AivoraPOS.Data;

#nullable disable

namespace AivoraPOS.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260718230000_AddSecurityLockoutFields")]
    public partial class AddSecurityLockoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSensitive",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FailedPasswordAttempts",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FailedPinAttempts",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccountLocked",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PinLockedUntilUtc",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSensitive",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "FailedPasswordAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FailedPinAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAccountLocked",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PinLockedUntilUtc",
                table: "Users");
        }
    }
}
