// File: Models/ClienteContatto.cs
// Rappresenta la tabella 'clienti_contatti' nel database.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("clienti_contatti")] // Mappa la classe alla tabella 'clienti_contatti'
    public class ClienteContatto
    {
        [Key] // Specifica che 'Id' è la chiave primaria
        [Column("id")] // Mappa la proprietà alla colonna 'id'
        public int Id { get; set; }

        [Required]
        [Column("id_cli")] // Mappa la proprietà alla colonna 'id_cli'
        public int IdCli { get; set; }

        [Column("predefinito")]
        public bool? Predefinito { get; set; } // '?' indica che il campo è nullable

        [Required]
        [Column("contatto")]
        [StringLength(100)] // NCHAR(100) in SQL Server è mappato a string con lunghezza fissa
        public string Contatto { get; set; } = string.Empty;

        [Column("telefono")]
        [StringLength(50)] // NCHAR(50)
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(50)] // NCHAR(50)
        public string? Email { get; set; }

        // Nuovo campo per indicare se il contatto è un dato di test
        [Column("is_test_data")]
        public bool IsTestData { get; set; } = false;

        // Proprietà di navigazione per la relazione molti-a-uno con Cliente
        [ForeignKey("IdCli")] // Specifica che 'IdCli' è la chiave esterna
        public Cliente? Cliente { get; set; }
    }
}
