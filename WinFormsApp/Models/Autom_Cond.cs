using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    [Table("autom_cond")]
    public class Autom_Cond
    {
        [Required]
        [Column("id_autom")]
        public int Id_Automezzo { get; set; }

        [Required]
        [Column("id_cond")]
        public int Id_Conducente { get; set; }

        public Automezzo Automezzo { get; set; }
        public Conducente Conducente { get; set; }
    }
}
