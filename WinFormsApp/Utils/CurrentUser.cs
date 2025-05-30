// File: Utils/CurrentUser.cs
// Questa classe statica memorizza l'utente attualmente loggato nell'applicazione.
namespace FormulariRif_G.Utils
{
    public static class CurrentUser
    {
        public static Models.Utente? LoggedInUser { get; private set; }

        /// <summary>
        /// Imposta l'utente attualmente loggato.
        /// </summary>
        /// <param name="user">L'oggetto Utente loggato.</param>
        public static void SetLoggedInUser(Models.Utente user)
        {
            LoggedInUser = user;
        }

        /// <summary>
        /// Resetta l'utente loggato (es. al logout).
        /// </summary>
        public static void Logout()
        {
            LoggedInUser = null;
        }

        /// <summary>
        /// Controlla se un utente è attualmente loggato.
        /// </summary>
        public static bool IsLoggedIn => LoggedInUser != null;

        /// <summary>
        /// Controlla se l'utente loggato è un amministratore.
        /// </summary>
        public static bool IsAdmin => LoggedInUser != null && (LoggedInUser.Admin ?? false);
    }
}
