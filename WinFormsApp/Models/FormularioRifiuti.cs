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
        [Column("id_produttore")] 
        public int IdProduttore { get; set; }

        [Required]
        [Column("id_produttore_indirizzo")] 
        public int IdProduttoreIndirizzo { get; set; }
        
        [Required]
        [Column("id_destinatario")] 
        public int IdDestinatario { get; set; }

        [Required]
        [Column("id_destinatario_indirizzo")] 
        public int IdDestinatarioIndirizzo { get; set; }

        [Required]
        [Column("id_trasportatore")] 
        public int IdTrasportatore { get; set; }

        [Required]
        [Column("id_trasportatore_indirizzo")] 
        public int IdTrasportatoreIndirizzo { get; set; }

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
        
        [StringLength(1)]
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

        [StringLength(2)]
        [Column("dest_r")]
        public string? Dest_R { get; set; }
        [StringLength(2)]
        [Column("dest_d")]
        public string? Dest_D { get; set; }

        [Column("detentore_rif")]
        public bool? Detentore_R { get; set; }

        // Proprietà di navigazione
        public Cliente? Produttore { get; set; }
        public ClienteIndirizzo? ProduttoreIndirizzo { get; set; }

        public Cliente? Destinatario { get; set; }
        public ClienteIndirizzo? DestinatarioIndirizzo { get; set; }

        public Cliente? Trasportatore { get; set; }
        public ClienteIndirizzo? TrasportatoreIndirizzo { get; set; }

        public Automezzo? Automezzo { get; set; }
    }
}
