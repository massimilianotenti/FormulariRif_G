using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariRif_G.Migrations
{
    /// <inheritdoc />
    public partial class Dest_RD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dest_d",
                table: "formulari_rifiuti",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dest_r",
                table: "formulari_rifiuti",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "detentore_rif",
                table: "formulari_rifiuti",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dest_d",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "dest_r",
                table: "formulari_rifiuti");

            migrationBuilder.DropColumn(
                name: "detentore_rif",
                table: "formulari_rifiuti");
        }
    }
}
