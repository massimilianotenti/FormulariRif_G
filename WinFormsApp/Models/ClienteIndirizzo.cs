// File: Models/ClienteIndirizzo.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    [Table("clienti_indirizzi")]
    public class ClienteIndirizzo
    {
        public int Id { get; set; }
        public int IdCli { get; set; }
        public string? Indirizzo { get; set; }
        public string? Comune { get; set; }
        public int? Cap { get; set; }
        public bool Predefinito { get; set; }
        public bool IsTestData { get; set; }

        // Navigation property per la relazione con Cliente
        public virtual Cliente Cliente { get; set; }

        [NotMapped]
        public string IndirizzoCompleto => $"{Indirizzo}, {Cap} {Comune}".Trim();
    }
}