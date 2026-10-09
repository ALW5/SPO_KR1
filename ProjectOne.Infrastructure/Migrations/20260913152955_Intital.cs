using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Intital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WareHouse",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ware_house", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "StorageCell",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ware_house_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number_storage_calls = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Depth = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: true),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_storage_cell", x => x.id);
                    table.ForeignKey(
                        name: "fk_storage_cell_ware_house_ware_house_id",
                        column: x => x.ware_house_id,
                        principalTable: "WareHouse",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentalAgreement",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rental_agreement", x => x.id);
                    table.ForeignKey(
                        name: "fk_rental_agreement_storage_cell_storage_id",
                        column: x => x.storage_id,
                        principalTable: "StorageCell",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_rental_agreement_storage_id",
                table: "RentalAgreement",
                column: "storage_id");

            migrationBuilder.CreateIndex(
                name: "ix_storage_cell_ware_house_id",
                table: "StorageCell",
                column: "ware_house_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalAgreement");

            migrationBuilder.DropTable(
                name: "StorageCell");

            migrationBuilder.DropTable(
                name: "WareHouse");
        }
    }
}
