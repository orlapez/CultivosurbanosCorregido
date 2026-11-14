using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Terpy.API.Migrations
{
    /// <inheritdoc />
    public partial class SimplificarContexto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Siembras_CantidadPlantasInicial",
                table: "Siembras");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Siembras_Estado",
                table: "Siembras");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrosSeguimiento_Datos",
                table: "RegistrosSeguimiento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_RegistrosSeguimiento_Humedad",
                table: "RegistrosSeguimiento");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cosechas_CantidadKg",
                table: "Cosechas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ActividadesCuidado_Tipo",
                table: "ActividadesCuidado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Siembras_CantidadPlantasInicial",
                table: "Siembras",
                sql: "[CantidadPlantasInicial] IS NULL OR [CantidadPlantasInicial] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Siembras_Estado",
                table: "Siembras",
                sql: "[Estado] IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrosSeguimiento_Datos",
                table: "RegistrosSeguimiento",
                sql: "[TemperaturaC] IS NOT NULL OR [HumedadSueloPorcentaje] IS NOT NULL OR NULLIF(LTRIM(RTRIM([Observacion])), N'') IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_RegistrosSeguimiento_Humedad",
                table: "RegistrosSeguimiento",
                sql: "[HumedadSueloPorcentaje] IS NULL OR [HumedadSueloPorcentaje] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cosechas_CantidadKg",
                table: "Cosechas",
                sql: "[CantidadKg] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ActividadesCuidado_Tipo",
                table: "ActividadesCuidado",
                sql: "[TipoActividad] IN (1, 2, 3, 4, 5)");
        }
    }
}
