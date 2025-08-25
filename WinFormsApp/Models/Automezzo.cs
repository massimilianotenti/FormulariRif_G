// File: Models/Automezzo.cs
// Rappresenta un automezzo nel sistema.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("automezzi")] // Mappa la classe alla tabella 'automezzi' nel database
    public class Automezzo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        [StringLength(20)] // La targa ha una lunghezza specifica
        [Column("targa")]
        public string Targa { get; set; } = string.Empty;

        public ICollection<Autom_Cond> AutomezziConducenti { get; set; }
    }
}
