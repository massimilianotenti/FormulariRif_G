// File: Models/Utente.cs
// Rappresenta la tabella 'utenti' nel database.
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FormulariRif_G.Models
{
    // Mappa la classe alla tabella 'utenti'
    [Table("utenti")] 
    public class Utente
    {
        [Key] 
        [Column("id")] 
        public int Id { get; set; }

        [Column("admin")]
        public bool? Admin { get; set; } 

        [Required]
        [Column("utente")]
        [StringLength(50)]
        public string NomeUtente { get; set; } = string.Empty; 

        // L'hash di BCrypt è una stringa più lunga
        [Required]
        [Column("password")]
        [StringLength(100)] 
        public string Password { get; set; } = string.Empty;

        // passwordsalt non è più necessario con BCrypt, ma mantenuto per compatibilità
        [Required]
        [Column("passwordsalt")]
        [StringLength(100)]
        public string PasswordSalt { get; set; } = string.Empty;

        // La proprietà PasswordSalt non è più necessaria con BCrypt, poiché il sale è incorporato
        // nell'hash della password. Se l'avevi in precedenza, dovrai rimuoverla e applicare una migrazione.
        // [Required]
        // [Column("passwordSalt")]
        // [StringLength(100)]
        // public string PasswordSalt { get; set; } = string.Empty;

        [Column("email")]
        [StringLength(50)]
        public string? Email { get; set; }

        [Column("must_change_password")]
        public bool MustChangePassword { get; set; }
    }
}
