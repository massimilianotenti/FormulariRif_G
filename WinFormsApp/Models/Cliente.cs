// File: Models/Cliente.cs
// Rappresenta un cliente nel sistema.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("clienti")] // Mappa la classe alla tabella 'clienti' nel database
    public class Cliente
    {
        [Key] // Specifica che Id è la chiave primaria
        public int Id { get; set; }

        [Required] // Campo obbligatorio
        [StringLength(255)] // Limita la lunghezza della stringa
        [Column("rag_soc")] // Mappa alla colonna 'rag_soc'
        public string RagSoc { get; set; } = string.Empty;

        // Rimosse le proprietà Indirizzo, Comune, Cap da qui.
        // Saranno gestite nella nuova tabella ClienteIndirizzo.

        [StringLength(16)] // Aggiunto Codice Fiscale
        [Column("codice_fiscale")]
        public string? CodiceFiscale { get; set; }

        [Column("is_test_data")] // Indica se il record è stato generato per test
        public bool IsTestData { get; set; }

        // Proprietà di navigazione per i contatti associati a questo cliente
        public ICollection<ClienteContatto> Contatti { get; set; } = new List<ClienteContatto>();

        // Nuova proprietà di navigazione per gli indirizzi associati a questo cliente
        public ICollection<ClienteIndirizzo> Indirizzi { get; set; } = new List<ClienteIndirizzo>();
    }
}
