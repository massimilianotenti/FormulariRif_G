// File: Models/ClienteIndirizzo.cs
// Rappresenta un indirizzo associato a un cliente.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("clienti_indirizzi")] // Mappa la classe alla tabella 'clienti_indirizzi' nel database
    public class ClienteIndirizzo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("id_cli")] // Chiave esterna per Cliente
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

        [Required]
        [Column("predefinito")] // Indica se è l'indirizzo predefinito per il cliente
        public bool Predefinito { get; set; }

        [Column("is_test_data")] // Indica se il record è stato generato per test
        public bool IsTestData { get; set; }

        // Proprietà di navigazione per il cliente associato
        public Cliente? Cliente { get; set; }
    }
}
