using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariRif_G.Migrations
{
    /// <inheritdoc />
    public partial class Tipo_su_cliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo",
                table: "clienti");

            migrationBuilder.AddColumn<int>(
                name: "tipo_id",
                table: "clienti",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descrizione = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clienti_tipo_id",
                table: "clienti",
                column: "tipo_id");

            migrationBuilder.AddForeignKey(
                name: "FK_clienti_tipo_tipo_id",
                table: "clienti",
                column: "tipo_id",
                principalTable: "tipo",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_clienti_tipo_tipo_id",
                table: "clienti");

            migrationBuilder.DropTable(
                name: "tipo");

            migrationBuilder.DropIndex(
                name: "IX_clienti_tipo_id",
                table: "clienti");

            migrationBuilder.DropColumn(
                name: "tipo_id",
                table: "clienti");

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                table: "clienti",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }
    }
}
