using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariRif_G.Migrations
{
    /// <inheritdoc />
    public partial class cambio_stato_fisico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "stato_fisico",
                table: "formulari_rifiuti",
                type: "TEXT",
                maxLength: 1,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "stato_fisico",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1,
                oldNullable: true);
        }
    }
}
