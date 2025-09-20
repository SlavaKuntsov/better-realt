using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flatly.Persistance.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class change_realEstateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "real_estates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "area_living",
                table: "real_estates",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "code",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_email",
                table: "real_estates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "contact_name",
                table: "real_estates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "contact_phones",
                table: "real_estates",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "real_estates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "headline",
                table: "real_estates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "images",
                table: "real_estates",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "price_rub",
                table: "real_estates",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_usd",
                table: "real_estates",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rooms",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "storey",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "storeys",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "real_estates",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "area_living",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "code",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "contact_email",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "contact_name",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "contact_phones",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "headline",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "images",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "price_rub",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "price_usd",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "rooms",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "storey",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "storeys",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "real_estates");
        }
    }
}
