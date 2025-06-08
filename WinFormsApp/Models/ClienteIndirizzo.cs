// File: Models/ClienteIndirizzo.cs
// Rappresenta un indirizzo associato a un cliente.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    // Mappa la classe alla tabella 'clienti_indirizzi' nel database
    [Table("clienti_indirizzi")] 
    public class ClienteIndirizzo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_cli")] 
        public int IdCli { get; set; }

        [Required]
        [StringLength(255)]
        [Column("indirizzo")]
        public string Indirizzo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("comune")]
        public string Comune { get; set; } = string.Empty;

        [Required]
        [Column("cap")]
        public int Cap { get; set; }
        
        [Column("predefinito")] 
        public bool Predefinito { get; set; }

        [Column("is_test_data")] 
        public bool IsTestData { get; set; }

        // Proprietà di navigazione per il cliente associato
        public Cliente? Cliente { get; set; }
    }
}
