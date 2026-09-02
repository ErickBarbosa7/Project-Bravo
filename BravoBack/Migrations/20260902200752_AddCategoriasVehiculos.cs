using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BravoBack.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriasVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasVehiculo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasVehiculo", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "CategoriasVehiculo",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Escolta" },
                    { 2, "Ejecutivo" },
                    { 3, "Carga" },
                    { 4, "Utilitario" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriasVehiculo");
        }
    }
}
