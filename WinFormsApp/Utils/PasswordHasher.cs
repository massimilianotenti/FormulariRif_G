// File: Utils/PasswordHasher.cs
// Questa classe fornisce funzionalità per l'hashing e la verifica delle password
// utilizzando la libreria BCrypt.NET-Next.
using BCrypt.Net; // Assicurati di aver installato il pacchetto NuGet BCrypt.Net-Next

namespace FormulariRif_G.Utils
{
    public static class PasswordHasher
    {
        public static string NewPasswordSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt(12); // Genera un nuovo sale con un costo di lavoro di 12
        }

        /// <summary>
        /// Genera l'hash di una password utilizzando BCrypt.
        /// Il sale viene generato e incorporato automaticamente nell'hash restituito da BCrypt.
        /// </summary>
        /// <param name="password">La password in chiaro da hashare.</param>
        /// <returns>L'hash della password (che include il sale).</returns>
        public static string HashPassword(string password, string passwordSalt)
        {
            // BCrypt genera automaticamente un sale casuale e lo incorpora nell'hash.
            // Il costo di lavoro (work factor) di default è 10, che è un buon compromesso.
            return BCrypt.Net.BCrypt.HashPassword(password, passwordSalt);
        }

        /// <summary>
        /// Verifica una password in chiaro confrontandola con un hash BCrypt memorizzato.
        /// </summary>
        /// <param name="password">La password in chiaro fornita dall'utente.</param>
        /// <param name="hashedPassword">L'hash della password memorizzato (generato da BCrypt).</param>
        /// <returns>True se la password corrisponde all'hash, altrimenti False.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // BCrypt verifica automaticamente la password usando il sale incorporato nell'hash.
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
