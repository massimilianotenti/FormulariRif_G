// File: Models/FormularioRifiuti.cs
// Rappresenta un formulario dei rifiuti.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("formulari_rifiuti")] // Mappa la classe alla tabella 'formulari_rifiuti' nel database
    public class FormularioRifiuti
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column("data")]
        public DateTime Data { get; set; }

        [Required]
        [Column("id_cli")] // Chiave esterna per Cliente
        public int IdCli { get; set; }

        [Required]
        [Column("id_cliente_indirizzo")] // Chiave esterna per ClienteIndirizzo
        public int IdClienteIndirizzo { get; set; }
        
        [StringLength(50)] // Numero del formulario alfanumerico
        [Column("numero_formulario")]
        public string NumeroFormulario { get; set; } = string.Empty;

        [Required]
        [Column("id_automezzo")] // Chiave esterna per Automezzo
        public int IdAutomezzo { get; set; }

        // Proprietà di navigazione
        public Cliente? Cliente { get; set; }
        public ClienteIndirizzo? ClienteIndirizzo { get; set; }
        public Automezzo? Automezzo { get; set; }
    }
}
