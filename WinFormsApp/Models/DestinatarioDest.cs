using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    [Table("destinatario_dest")]
    public class DestinatarioDest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Tipo { get; set; }

        [Required]
        [StringLength(2)]
        [Column("desc")]
        public string Desc { get; set; }        
    }
}
