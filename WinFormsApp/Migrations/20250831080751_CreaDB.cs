using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormulariRif_G.Migrations
{
    /// <inheritdoc />
    public partial class CreaDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "automezzi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descrizione = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    targa = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automezzi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clienti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    rag_soc = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    partita_iva = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    codice_fiscale = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    iscrizione_albo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    auto_comunicazione = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clienti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "conducenti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descrizione = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    contatto = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conducenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "configurazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    dati_test = table.Column<bool>(type: "INTEGER", nullable: true),
                    rag_soc1 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    rag_soc2 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    indirizzo = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    comune = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    cap = table.Column<int>(type: "INTEGER", nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    partita_iva = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    codice_fiscale = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    dest_numero_iscrizione_albo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    dest_r = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    dest_d = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    dest_auto_comunic = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    dest_tipo1 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    dest_tipo2 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    numero_iscrizione_albo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    data_iscrizione_albo = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configurazione", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rimorchi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    descrizione = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    targa = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rimorchi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "utenti",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    admin = table.Column<bool>(type: "INTEGER", nullable: true),
                    utente = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    passwordsalt = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    must_change_password = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utenti", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "clienti_contatti",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_cli = table.Column<int>(type: "INTEGER", nullable: false),
                    predefinito = table.Column<bool>(type: "INTEGER", nullable: false),
                    contatto = table.Column<string>(type: "nchar(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "nchar(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "nchar(50)", maxLength: 50, nullable: true),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clienti_contatti", x => x.id);
                    table.ForeignKey(
                        name: "FK_clienti_contatti_clienti_id_cli",
                        column: x => x.id_cli,
                        principalTable: "clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clienti_indirizzi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    id_cli = table.Column<int>(type: "INTEGER", nullable: false),
                    indirizzo = table.Column<string>(type: "TEXT", nullable: true),
                    comune = table.Column<string>(type: "TEXT", nullable: true),
                    cap = table.Column<int>(type: "INTEGER", nullable: true),
                    predefinito = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clienti_indirizzi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clienti_indirizzi_clienti_id_cli",
                        column: x => x.id_cli,
                        principalTable: "clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "autom_cond",
                columns: table => new
                {
                    id_autom = table.Column<int>(type: "INTEGER", nullable: false),
                    id_cond = table.Column<int>(type: "INTEGER", nullable: false),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_autom_cond", x => new { x.id_autom, x.id_cond });
                    table.ForeignKey(
                        name: "FK_autom_cond_automezzi_id_autom",
                        column: x => x.id_autom,
                        principalTable: "automezzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_autom_cond_conducenti_id_cond",
                        column: x => x.id_cond,
                        principalTable: "conducenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "autom_rim",
                columns: table => new
                {
                    id_autom = table.Column<int>(type: "INTEGER", nullable: false),
                    id_rim = table.Column<int>(type: "INTEGER", nullable: false),
                    is_test_data = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_autom_rim", x => new { x.id_autom, x.id_rim });
                    table.ForeignKey(
                        name: "FK_autom_rim_automezzi_id_autom",
                        column: x => x.id_autom,
                        principalTable: "automezzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_autom_rim_rimorchi_id_rim",
                        column: x => x.id_rim,
                        principalTable: "rimorchi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "formulari_rifiuti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    id_produttore = table.Column<int>(type: "INTEGER", nullable: false),
                    id_produttore_indirizzo = table.Column<int>(type: "INTEGER", nullable: false),
                    id_destinatario = table.Column<int>(type: "INTEGER", nullable: false),
                    id_destinatario_indirizzo = table.Column<int>(type: "INTEGER", nullable: false),
                    id_trasportatore = table.Column<int>(type: "INTEGER", nullable: false),
                    id_trasportatore_indirizzo = table.Column<int>(type: "INTEGER", nullable: false),
                    numero_formulario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    id_automezzo = table.Column<int>(type: "INTEGER", nullable: false),
                    codice_eer = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    stato_fisico = table.Column<int>(type: "INTEGER", nullable: true),
                    provenienza = table.Column<int>(type: "INTEGER", nullable: true),
                    caratteristiche_pericolosita = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    descrizione = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    quantita = table.Column<decimal>(type: "TEXT", nullable: true),
                    kg_litri = table.Column<int>(type: "INTEGER", nullable: true),
                    peso_verificato = table.Column<bool>(type: "INTEGER", nullable: true),
                    numero_colli = table.Column<int>(type: "INTEGER", nullable: true),
                    alla_rinfusa = table.Column<bool>(type: "INTEGER", nullable: true),
                    caratteristiche_chimiche = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formulari_rifiuti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_automezzi_id_automezzo",
                        column: x => x.id_automezzo,
                        principalTable: "automezzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_clienti_id_destinatario",
                        column: x => x.id_destinatario,
                        principalTable: "clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_clienti_id_produttore",
                        column: x => x.id_produttore,
                        principalTable: "clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_clienti_id_trasportatore",
                        column: x => x.id_trasportatore,
                        principalTable: "clienti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_clienti_indirizzi_id_destinatario_indirizzo",
                        column: x => x.id_destinatario_indirizzo,
                        principalTable: "clienti_indirizzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_clienti_indirizzi_id_produttore_indirizzo",
                        column: x => x.id_produttore_indirizzo,
                        principalTable: "clienti_indirizzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_formulari_rifiuti_clienti_indirizzi_id_trasportatore_indirizzo",
                        column: x => x.id_trasportatore_indirizzo,
                        principalTable: "clienti_indirizzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_autom_cond_id_cond",
                table: "autom_cond",
                column: "id_cond");

            migrationBuilder.CreateIndex(
                name: "IX_autom_rim_id_rim",
                table: "autom_rim",
                column: "id_rim");

            migrationBuilder.CreateIndex(
                name: "IX_clienti_contatti_id_cli",
                table: "clienti_contatti",
                column: "id_cli");

            migrationBuilder.CreateIndex(
                name: "IX_clienti_indirizzi_id_cli",
                table: "clienti_indirizzi",
                column: "id_cli");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_automezzo",
                table: "formulari_rifiuti",
                column: "id_automezzo");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_destinatario",
                table: "formulari_rifiuti",
                column: "id_destinatario");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_destinatario_indirizzo",
                table: "formulari_rifiuti",
                column: "id_destinatario_indirizzo");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_produttore",
                table: "formulari_rifiuti",
                column: "id_produttore");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_produttore_indirizzo",
                table: "formulari_rifiuti",
                column: "id_produttore_indirizzo");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_trasportatore",
                table: "formulari_rifiuti",
                column: "id_trasportatore");

            migrationBuilder.CreateIndex(
                name: "IX_formulari_rifiuti_id_trasportatore_indirizzo",
                table: "formulari_rifiuti",
                column: "id_trasportatore_indirizzo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "autom_cond");

            migrationBuilder.DropTable(
                name: "autom_rim");

            migrationBuilder.DropTable(
                name: "clienti_contatti");

            migrationBuilder.DropTable(
                name: "configurazione");

            migrationBuilder.DropTable(
                name: "formulari_rifiuti");

            migrationBuilder.DropTable(
                name: "utenti");

            migrationBuilder.DropTable(
                name: "conducenti");

            migrationBuilder.DropTable(
                name: "rimorchi");

            migrationBuilder.DropTable(
                name: "automezzi");

            migrationBuilder.DropTable(
                name: "clienti_indirizzi");

            migrationBuilder.DropTable(
                name: "clienti");
        }
    }
}
