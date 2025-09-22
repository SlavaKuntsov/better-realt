using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flatly.Persistance.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class upd_realEstate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "appliances",
                table: "real_estates",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<double>(
                name: "area_kitchen",
                table: "real_estates",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "balcony_type",
                table: "real_estates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "building_number",
                table: "real_estates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "building_year",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "furniture",
                table: "real_estates",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "house_number",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "housing_rent",
                table: "real_estates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "real_estates",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "layout",
                table: "real_estates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lease_period",
                table: "real_estates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "real_estates",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "new_again_date",
                table: "real_estates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "overhaul_year",
                table: "real_estates",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "paid",
                table: "real_estates",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prepayment",
                table: "real_estates",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_byn",
                table: "real_estates",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_eur",
                table: "real_estates",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_rub_rus",
                table: "real_estates",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "raise_date",
                table: "real_estates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "repair_state",
                table: "real_estates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seller",
                table: "real_estates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_name",
                table: "real_estates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "toilet",
                table: "real_estates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "town_district_name",
                table: "real_estates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "town_name",
                table: "real_estates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "town_sub_district_name",
                table: "real_estates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "views_count",
                table: "real_estates",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "appliances",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "area_kitchen",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "balcony_type",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "building_number",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "building_year",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "furniture",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "house_number",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "housing_rent",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "layout",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "lease_period",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "new_again_date",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "overhaul_year",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "paid",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "prepayment",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "price_byn",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "price_eur",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "price_rub_rus",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "raise_date",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "repair_state",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "seller",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "street_name",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "toilet",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "town_district_name",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "town_name",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "town_sub_district_name",
                table: "real_estates");

            migrationBuilder.DropColumn(
                name: "views_count",
                table: "real_estates");
        }
    }
}
