using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormulariRif_G.Models
{
    [Table("autom_rim")]
    public class Autom_Rim
    {
        [Required]
        [Column("id_autom")]
        public int Id_Automezzo { get; set; }

        [Required]
        [Column("id_rim")]
        public int Id_Rimorchio { get; set; }

        [Column("is_test_data")]
        public bool IsTestData { get; set; }

        public Automezzo Automezzo { get; set; }
        public Rimorchio Rimorchio { get; set; }

    }
}
