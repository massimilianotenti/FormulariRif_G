using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariRif_G.Migrations
{
    /// <inheritdoc />
    public partial class Indirizzi_Unita_Locale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinatarioIndUlId",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProduttoreIndUlId",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_destinatario_ind_ul",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "id_produttore_ind_ul",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_DestinatarioIndUlId",
                table: "formulari_rifiuti",
                column: "DestinatarioIndUlId");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_ProduttoreIndUlId",
                table: "formulari_rifiuti",
                column: "ProduttoreIndUlId");

            migrationBuilder.AddForeignKey(
                name: "FK_formulari_rifiuti_clienti_indirizzi_DestinatarioIndUlId",
                table: "formulari_rifiuti",
                column: "DestinatarioIndUlId",
                principalTable: "clienti_indirizzi",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_formulari_rifiuti_clienti_indirizzi_ProduttoreIndUlId",
                table: "formulari_rifiuti",
                column: "ProduttoreIndUlId",
                principalTable: "clienti_indirizzi",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_formulari_rifiuti_clienti_indirizzi_DestinatarioIndUlId",
                table: "formulari_rifiuti");

            migrationBuilder.DropForeignKey(
                name: "FK_formulari_rifiuti_clienti_indirizzi_ProduttoreIndUlId",
                table: "formulari_rifiuti");

            migrationBuilder.DropIndex(
                name: "IX_formulari_rifiuti_DestinatarioIndUlId",
                table: "formulari_rifiuti");

            migrationBuilder.DropIndex(
                name: "IX_formulari_rifiuti_ProduttoreIndUlId",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "DestinatarioIndUlId",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "ProduttoreIndUlId",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "id_destinatario_ind_ul",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "id_produttore_ind_ul",
                table: "formulari_rifiuti");
        }
    }
}
