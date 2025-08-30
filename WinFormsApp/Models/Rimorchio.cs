using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    [Table("rimorchi")]
    public class Rimorchio
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

        [Column("is_test_data")]
        public bool IsTestData { get; set; }

        public ICollection<Autom_Rim> RimorchioAutomezzi  { get; set; }
    }
}
