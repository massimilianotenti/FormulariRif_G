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
        [Column("id_cli")] 
        public int IdCli { get; set; }

        [Required]
        [Column("id_cliente_indirizzo")] 
        public int IdClienteIndirizzo { get; set; }
        
        [StringLength(50)] 
        [Column("numero_formulario")]
        public string? NumeroFormulario { get; set; } = string.Empty;

        [Required]
        [Column("id_automezzo")] 
        public int IdAutomezzo { get; set; }

        //Caratteristiche del rifiuto
        [StringLength(10)]
        [Column("codice_eer")]
        public string? CodiceEER { get; set; }

        [StringLength(3)]
        [Column("stato_fisico")]
        public string? SatoFisico { get; set; } 

        [Column("provenienza")]
        //1 - Urbano / 2 - Speciale
        public int? Provenienza { get; set; }    

        [StringLength(25)]
        [Column("caratteristiche_pericolosita")]
        public string? CarattPericolosita { get; set; }

        [StringLength(50)]
        [Column("descrizione")]
        public string? Descrizione { get; set; }

        [Column("quantita")]
        public decimal? Quantita { get; set; }

        [Column("kg_litri")]
        //1 - Kg / 2 - Litri
        public int? Kg_Lt { get; set; }

        [Column("peso_verificato")]
        public bool? PesoVerificato { get; set; }

        [Column("numero_colli")]
        public int? NumeroColli { get; set; }

        [Column("alla_rinfusa")]
        public bool? AllaRinfusa { get; set; }

        [StringLength(25)]
        [Column("caratteristiche_chimiche")]
        public string? CaratteristicheChimiche { get; set; }

        // Proprietà di navigazione
        public Cliente? Cliente { get; set; }
        public ClienteIndirizzo? ClienteIndirizzo { get; set; }
        public Automezzo? Automezzo { get; set; }
    }
}
