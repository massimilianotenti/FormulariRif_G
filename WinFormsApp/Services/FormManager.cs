// File: Services/FormManager.cs
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FormulariRif_G.Service
{
    /// <summary>
    /// Gestisce l'apertura e l'attivazione delle form nell'applicazione,
    /// assicurando che non vengano create multiple istanze della stessa form
    /// se già aperta, e che le risorse delle form chiuse vengano correttamente liberate.
    /// </summary>
    public class FormManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<Type, Form> _openForms = new Dictionary<Type, Form>();

        public FormManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Mostra o attiva una form specificata dal suo tipo.
        /// Se la form è già aperta, la porta in primo piano.
        /// Altrimenti, crea una nuova istanza, la mostra e la aggiunge alla lista delle form aperte.
        /// </summary>
        /// <typeparam name="TForm">Il tipo della form da mostrare/attivare.</typeparam>
        /// <returns>L'istanza della form (esistente o appena creata).</returns>
        public TForm ShowOrActivate<TForm>() where TForm : Form
        {
            var formType = typeof(TForm);

            // Cerca se la form è già aperta
            if (_openForms.TryGetValue(formType, out Form? existingForm))
            {
                // Se la form esiste ma è stata disposta (es. chiusa dall'utente), rimuovila.
                // Questo può accadere se il form viene chiuso manualmente dall'utente ma non rimosso dalla dictionary.
                if (existingForm.IsDisposed)
                {
                    _openForms.Remove(formType);
                }
                else
                {
                    // La form esiste e non è disposta, la attiviamo e la portiamo in primo piano
                    if (existingForm.WindowState == FormWindowState.Minimized)
                    {
                        existingForm.WindowState = FormWindowState.Normal;
                    }
                    existingForm.BringToFront();
                    existingForm.Activate();
                    return (TForm)existingForm;
                }
            }

            // La form non è aperta o è stata disposta, quindi ne creiamo una nuova istanza
            var newForm = _serviceProvider.GetRequiredService<TForm>();

            // Gestiamo l'evento FormClosed per rimuovere la form dalla lista quando viene chiusa
            newForm.FormClosed += (sender, e) =>
            {
                if (sender is TForm closedForm)
                {
                    _openForms.Remove(typeof(TForm));
                    // Importante: Dispose() la form manualmente qui se la form è stata creata senza "using"
                    // e non è gestita automaticamente dal DI container per il suo lifetime.
                    // Dato che stiamo usando AddTransient per le form, è buona norma disporle esplicitamente.
                    closedForm.Dispose();
                }
            };

            // Aggiungiamo la nuova form alla lista e la mostriamo
            _openForms.Add(formType, newForm);
            newForm.Show();

            return newForm;
        }

        /// <summary>
        /// Restituisce un'istanza di una form già aperta di un determinato tipo.
        /// Utile per accedere a una form esistente senza attivarla necessariamente.
        /// </summary>
        /// <typeparam name="TForm">Il tipo della form da cercare.</typeparam>
        /// <returns>L'istanza della form se aperta e non disposta; altrimenti, null.</returns>
        public TForm? GetOpenForm<TForm>() where TForm : Form
        {
            var formType = typeof(TForm);
            if (_openForms.TryGetValue(formType, out Form? form))
            {
                if (form.IsDisposed)
                {
                    _openForms.Remove(formType);
                    return null;
                }
                return (TForm)form;
            }
            return null;
        }

        /// <summary>
        /// Chiude tutte le form gestite dal FormManager.
        /// </summary>
        public void CloseAllForms()
        {
            // Creiamo una copia della lista per evitare problemi di enumerazione
            // quando modifichiamo la dictionary durante l'iterazione.
            var formsToClose = _openForms.Values.ToList();
            foreach (var form in formsToClose)
            {
                if (!form.IsDisposed)
                {
                    form.Close();
                }
            }
            _openForms.Clear(); // Assicurati che la dictionary sia pulita
        }
    }
}