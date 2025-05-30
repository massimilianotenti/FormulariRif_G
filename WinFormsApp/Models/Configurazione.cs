// File: Models/Configurazione.cs
// Rappresenta le impostazioni di configurazione dell'applicazione, inclusi i dati dell'azienda.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("configurazione")] // Mappa la classe alla tabella 'configurazione' nel database
    public class Configurazione
    {
        [Key] // Specifica che Id è la chiave primaria
        public int Id { get; set; }

        [Column("dati_test")] // Mappa alla colonna 'dati_test'
        public bool? DatiTest { get; set; } // Indica se i dati di test sono abilitati

        [StringLength(255)] // Limita la lunghezza della stringa
        [Column("rag_soc")] // Mappa alla colonna 'rag_soc'
        public string? RagSoc { get; set; }

        [StringLength(255)]
        [Column("indirizzo")]
        public string? Indirizzo { get; set; }

        [StringLength(100)]
        [Column("comune")]
        public string? Comune { get; set; }

        [Column("cap")]
        public int Cap { get; set; }

        [StringLength(100)]
        [Column("email")]
        public string? Email { get; set; }

        // Nuove proprietà per la configurazione aziendale
        [StringLength(20)]
        [Column("partita_iva")]
        public string? PartitaIva { get; set; }

        [StringLength(16)] // Codice Fiscale italiano ha 16 caratteri
        [Column("codice_fiscale")]
        public string? CodiceFiscale { get; set; }

        [StringLength(50)]
        [Column("numero_iscrizione_albo")]
        public string? NumeroIscrizioneAlbo { get; set; }

        [Column("data_iscrizione_albo")]
        public DateTime? DataIscrizioneAlbo { get; set; }
    }
}
