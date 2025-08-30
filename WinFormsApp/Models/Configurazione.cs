// File: Models/Configurazione.cs
// Rappresenta le impostazioni di configurazione dell'applicazione, inclusi i dati dell'azienda.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    // Mappa la classe alla tabella 'configurazione' nel database
    [Table("configurazione")] 
    public class Configurazione
    {
        [Key] 
        public int Id { get; set; }

        // Indica se i dati di test sono abilitati
        [Column("dati_test")] 
        public bool? DatiTest { get; set; }

        //Dati dell'azienda
        [StringLength(255)] 
        [Column("rag_soc1")] 
        public string RagSoc1 { get; set; }

        [StringLength(255)]
        [Column("rag_soc2")]
        public string? RagSoc2 { get; set; }

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
        
        [StringLength(20)]
        [Column("partita_iva")]
        public string? PartitaIva { get; set; }

        [StringLength(16)]
        [Column("codice_fiscale")]
        public string? CodiceFiscale { get; set; }

        // Dati per il destinatario
        [StringLength(50)]
        [Column("dest_numero_iscrizione_albo")]
        public string? DestNumeroIscrizioneAlbo { get; set; }

        [StringLength(10)]
        [Column("dest_r")]
        public string? DestR { get; set; }

        [StringLength(10)]
        [Column("dest_d")]
        public string? DestD { get; set; }

        [StringLength(30)]
        [Column("dest_auto_comunic")]
        public string? DestAutoComunic { get; set; }

        [StringLength(50)]
        [Column("dest_tipo1")]
        public string? DestTipo1 { get; set; }

        [StringLength(50)]
        [Column("dest_tipo2")]
        public string? DestTipo2 { get; set; }


        // Dati per il trasportatore
        [StringLength(50)]
        [Column("numero_iscrizione_albo")]
        public string? NumeroIscrizioneAlbo { get; set; }

        [Column("data_iscrizione_albo")]
        public DateTime? DataIscrizioneAlbo { get; set; }
    }
}
