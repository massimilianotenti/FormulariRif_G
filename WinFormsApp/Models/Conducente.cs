using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    [Table("Conducenti")]
    public class Conducente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(250)]
        [Column("descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [StringLength(50)]
        [Column("contatto")]
        public string Contatto { get; set; } = string.Empty;

        [Required]
        [Column("tipo")]
        public int Tipo { get; set; }

        public ICollection<Autom_Cond> ConducentiAutomezzi { get; set; }
    }
}
