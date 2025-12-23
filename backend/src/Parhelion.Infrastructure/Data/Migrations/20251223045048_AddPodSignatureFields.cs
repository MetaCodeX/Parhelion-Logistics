using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parhelion.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPodSignatureFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "ShipmentDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "ShipmentDocuments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "ShipmentDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignatureBase64",
                table: "ShipmentDocuments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SignatureLatitude",
                table: "ShipmentDocuments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SignatureLongitude",
                table: "ShipmentDocuments",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedAt",
                table: "ShipmentDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedByName",
                table: "ShipmentDocuments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "SignatureBase64",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "SignatureLatitude",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "SignatureLongitude",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "SignedAt",
                table: "ShipmentDocuments");

            migrationBuilder.DropColumn(
                name: "SignedByName",
                table: "ShipmentDocuments");
        }
    }
}
