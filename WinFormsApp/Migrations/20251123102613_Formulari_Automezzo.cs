using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariRif_G.Migrations
{
    /// <inheritdoc />
    public partial class Formulari_Automezzo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConducenteId",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RimorchioId",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_conducente",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_rimorchio",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_ConducenteId",
                table: "formulari_rifiuti",
                column: "ConducenteId");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_RimorchioId",
                table: "formulari_rifiuti",
                column: "RimorchioId");

            migrationBuilder.AddForeignKey(
                name: "FK_formulari_rifiuti_conducenti_ConducenteId",
                table: "formulari_rifiuti",
                column: "ConducenteId",
                principalTable: "conducenti",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_formulari_rifiuti_rimorchi_RimorchioId",
                table: "formulari_rifiuti",
                column: "RimorchioId",
                principalTable: "rimorchi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_formulari_rifiuti_conducenti_ConducenteId",
                table: "formulari_rifiuti");

            migrationBuilder.DropForeignKey(
                name: "FK_formulari_rifiuti_rimorchi_RimorchioId",
                table: "formulari_rifiuti");

            migrationBuilder.DropIndex(
                name: "IX_formulari_rifiuti_ConducenteId",
                table: "formulari_rifiuti");

            migrationBuilder.DropIndex(
                name: "IX_formulari_rifiuti_RimorchioId",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "ConducenteId",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "RimorchioId",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "id_conducente",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "id_rimorchio",
                table: "formulari_rifiuti");
        }
    }
}
