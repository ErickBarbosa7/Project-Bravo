using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BravoBack.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoVehiculo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BitacorasViaje");

            migrationBuilder.CreateTable(
                name: "CatalogoVehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Marca = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IntervaloServicioKm = table.Column<int>(type: "integer", nullable: false),
                    FotoUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogoVehiculos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CatalogoVehiculos",
                columns: new[] { "Id", "Anio", "Categoria", "FotoUrl", "IntervaloServicioKm", "Marca", "Modelo" },
                values: new object[,]
                {
                    { 1, 2024, "Escolta", "assets/vehiculos/vehiculo-suburban.png", 10000, "Chevrolet", "Suburban" },
                    { 2, 2023, "Escolta", "assets/vehiculos/vehiculo-placeholder.png", 10000, "Chevrolet", "Tahoe" },
                    { 3, 2024, "Ejecutivo", "assets/vehiculos/vehiculo-placeholder.png", 12000, "Jeep", "Grand Cherokee" },
                    { 4, 2022, "Carga", "assets/vehiculos/vehiculo-placeholder.png", 15000, "Toyota", "Hilux" },
                    { 5, 2024, "Utilitario", "assets/vehiculos/vehiculo-placeholder.png", 10000, "Nissan", "Sentra" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogoVehiculos");

            migrationBuilder.CreateTable(
                name: "BitacorasViaje",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConductorId = table.Column<string>(type: "text", nullable: false),
                    VehiculoId = table.Column<int>(type: "integer", nullable: false),
                    Destino = table.Column<string>(type: "text", nullable: true),
                    FechaLlegada = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FechaSalida = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    KilometrajeLlegada = table.Column<int>(type: "integer", nullable: true),
                    KilometrajeSalida = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BitacorasViaje", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BitacorasViaje_AspNetUsers_ConductorId",
                        column: x => x.ConductorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BitacorasViaje_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BitacorasViaje_ConductorId",
                table: "BitacorasViaje",
                column: "ConductorId");

            migrationBuilder.CreateIndex(
                name: "IX_BitacorasViaje_VehiculoId",
                table: "BitacorasViaje",
                column: "VehiculoId");
        }
    }
}
