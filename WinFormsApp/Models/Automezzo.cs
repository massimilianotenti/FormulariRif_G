// File: Models/Automezzo.cs
// Rappresenta un automezzo nel sistema.
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("automezzi")] 
    public class Automezzo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        [StringLength(20)] 
        [Column("targa")]
        public string Targa { get; set; } = string.Empty;

        [Column("is_test_data")]
        public bool IsTestData { get; set; }
        
        [NotMapped] // Dice a Entity Framework di ignorare questa proprietà.
        [DisplayName("Conducenti")]
        public int NumeroConducenti { get; set; }

        [NotMapped] // Dice a Entity Framework di ignorare questa proprietà.
        [DisplayName("Rimorchi")]
        public int NumeroRimorchi { get; set; }

        public ICollection<Autom_Cond> AutomezziConducenti { get; set; }

        public ICollection<Autom_Rim> AutomezziRimorchi { get; set; }
    }
}
