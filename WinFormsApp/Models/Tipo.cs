using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    [Table("tipo")]
    public class Tipo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column("descrizione")]
        public required string Descrizione { get; set; }
    }
}
