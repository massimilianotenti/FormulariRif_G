// File: Models/ClienteContatto.cs
// Rappresenta la tabella 'clienti_contatti' nel database.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    // Mappa la classe alla tabella 'clienti_contatti'
    [Table("clienti_contatti")] 
    public class ClienteContatto
    {
        [Key] 
        [Column("id")] 
        public int Id { get; set; }

        [Required]
        [Column("id_cli")] 
        public int IdCli { get; set; }

        [Column("predefinito")]
        public bool Predefinito { get; set; } 

        [Required]
        [Column("contatto")]
        [StringLength(100)] 
        public string Contatto { get; set; } = string.Empty;

        [Column("telefono")]
        [StringLength(50)] 
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(50)] 
        public string? Email { get; set; }

        // Nuovo campo per indicare se il contatto è un dato di test
        [Column("is_test_data")]
        public bool IsTestData { get; set; } = false;

        // Proprietà di navigazione per la relazione molti-a-uno con Cliente
        [ForeignKey("IdCli")] 
        public Cliente? Cliente { get; set; }
    }
}
