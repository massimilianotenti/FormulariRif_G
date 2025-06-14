// File: Models/Cliente.cs
// Rappresenta un cliente nel sistema.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("clienti")] // Mappa la classe alla tabella 'clienti' nel database
    public class Cliente
    {
        [Key] 
        public int Id { get; set; }

        [Required] 
        [StringLength(255)] 
        [Column("rag_soc")] 
        public string RagSoc { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("partita_iva")]
        public string? PartitaIva { get; set; }

        [StringLength(16)] 
        [Column("codice_fiscale")]
        public string? CodiceFiscale { get; set; }

        [StringLength(50)]
        [Column("iscrizione_albo")]
        public string? Iscrizione_Albo { get; set; }

        [StringLength(50)]
        [Column("auto_comunicazione")]
        public string? Auto_Comunicazione { get; set; }

        [StringLength(50)]
        [Column("tipo")]
        public string? Tipo { get; set; }

        [Column("is_test_data")] 
        public bool IsTestData { get; set; }

        // Proprietà di navigazione per i contatti associati a questo cliente
        public ICollection<ClienteContatto> Contatti { get; set; } = new List<ClienteContatto>();

        // Nuova proprietà di navigazione per gli indirizzi associati a questo cliente
        public ICollection<ClienteIndirizzo> Indirizzi { get; set; } = new List<ClienteIndirizzo>();
    }
}
