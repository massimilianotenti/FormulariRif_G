// File: Forms/AutomezziDetailForm.cs
// Questo form permette di inserire o modificare un singolo automezzo.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System; // Per EventArgs e Exception
using System.Windows.Forms; // Per Form, MessageBox, DialogResult
using System.Threading.Tasks; // Per Task
using System.Collections.Generic; // Per List<T>
using System.Linq; // Per LINQ

namespace FormulariRif_G.Forms
{
    public partial class AutomezziDetailForm : Form
    {
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private readonly IGenericRepository<Autom_Cond> _automCondRepository;
        private readonly IGenericRepository<Rimorchio> _rimorchioRepository;
        private readonly IGenericRepository<Autom_Rim> _automRimRepository;

        private List<Conducente> _allConducenti = new List<Conducente>(); // Campo per memorizzare tutti i conducenti
        private List<Rimorchio> _allRimorchi = new List<Rimorchio>(); // Campo per memorizzare tutti i rimorchi
        private readonly System.Windows.Forms.Timer _conducenteSearchDebounceTimer; // Timer per il ritardo nella ricerca
        private readonly System.Windows.Forms.Timer _rimorchioSearchDebounceTimer; // Timer per il ritardo nella ricerca

        // Rimosse le flag, non più necessarie con il nuovo approccio
        // private bool _isConducenteSelectionCommitted = false;
        // private bool _isRimorchioSelectionCommitted = false;
        private Automezzo? _currentAutomezzo;

        // Dichiarazioni dei controlli (corrispondenti al tuo Designer.cs)
        private System.Windows.Forms.Label lblDescrizione;
        private System.Windows.Forms.TextBox txtDescrizione;
        private System.Windows.Forms.Label lblTarga;
        private System.Windows.Forms.TextBox txtTarga;
        private System.Windows.Forms.Button btnSalva;
        // Se hai un pulsante Annulla nel tuo designer, aggiungilo qui:
        private System.Windows.Forms.Button btnAnnulla;

        // --- Controlli per la gestione dei Conducenti ---
        private System.Windows.Forms.TextBox txtConducenteSearch;
        private System.Windows.Forms.Label lblCercaConducente;
        private System.Windows.Forms.ComboBox cmbConducentiResult;
        private System.Windows.Forms.Button btnAggiungiConducente;
        private System.Windows.Forms.Label lblConducentiAssociati;
        private System.Windows.Forms.ListBox lstConducentiAssociati;
        private System.Windows.Forms.Button btnRimuoviConducente;

        // --- Controlli per la gestione dei Rimorchi ---
        private System.Windows.Forms.TextBox txtRimorchioSearch;
        private System.Windows.Forms.Label lblCercaRimorchio;
        private System.Windows.Forms.ComboBox cmbRimorchiResult;
        private System.Windows.Forms.Button btnAggiungiRimorchio;
        private System.Windows.Forms.Label lblRimorchiAssociati;
        private System.Windows.Forms.ListBox lstRimorchiAssociati;
        private System.Windows.Forms.Button btnRimuoviRimorchio;

        public AutomezziDetailForm(IGenericRepository<Automezzo> automezzoRepository, IGenericRepository<Conducente> conducenteRepository, IGenericRepository<Autom_Cond> automCondRepository, IGenericRepository<Rimorchio> rimorchioRepository, IGenericRepository<Autom_Rim> automRimRepository)
        {
            InitializeComponent();
            _automezzoRepository = automezzoRepository;

            _conducenteRepository = conducenteRepository;
            _automCondRepository = automCondRepository;
            _rimorchioRepository = rimorchioRepository;
            _automRimRepository = automRimRepository;

            // Collega gli handler degli eventi ai pulsanti.
            // Questi collegamenti sono stati spostati qui dal Designer.cs.
            if (btnSalva != null) btnSalva.Click += btnSalvaClick;
            // Se hai un pulsante Annulla, scommenta o aggiungi la riga qui sotto:
            if (btnAnnulla != null) btnAnnulla.Click += btnAnnullaClick;
            if (btnAggiungiConducente != null) btnAggiungiConducente.Click += btnAggiungiConducente_Click;
            if (btnRimuoviConducente != null) btnRimuoviConducente.Click += btnRimuoviConducente_Click;
            // Collega l'evento TextChanged al nuovo TextBox di ricerca
            if (txtConducenteSearch != null)
            {
                txtConducenteSearch.TextChanged += TxtConducenteSearch_TextChanged;
                txtConducenteSearch.KeyDown += TxtConducenteSearch_KeyDown;
            }
            if (cmbConducentiResult != null)
            {
                cmbConducentiResult.KeyDown += CmbConducentiResult_KeyDown;
            }
            if (btnAggiungiRimorchio != null) btnAggiungiRimorchio.Click += btnAggiungiRimorchio_Click;
            if (btnRimuoviRimorchio != null) btnRimuoviRimorchio.Click += btnRimuoviRimorchio_Click;
            if (txtRimorchioSearch != null)
            {
                txtRimorchioSearch.TextChanged += TxtRimorchioSearch_TextChanged;
                txtRimorchioSearch.KeyDown += TxtRimorchioSearch_KeyDown;
            }
            if (cmbRimorchiResult != null)
            {
                cmbRimorchiResult.KeyDown += CmbRimorchiResult_KeyDown;
            }

            // Inizializza il timer per il "debouncing" della ricerca
            _conducenteSearchDebounceTimer = new System.Windows.Forms.Timer();
            _conducenteSearchDebounceTimer.Interval = 300; // Ritardo di 300ms
            _conducenteSearchDebounceTimer.Tick += PerformConducenteFiltering;

            // Inizializza il timer per la ricerca dei rimorchi
            _rimorchioSearchDebounceTimer = new System.Windows.Forms.Timer();
            _rimorchioSearchDebounceTimer.Interval = 300;
            _rimorchioSearchDebounceTimer.Tick += PerformRimorchioFiltering;
        }

        /// <summary>
        /// Imposta l'automezzo da visualizzare o modificare.
        /// </summary>
        /// <param name="automezzo">L'oggetto Automezzo.</param>
        public async void SetAutomezzo(Automezzo automezzo)
        {
            _currentAutomezzo = automezzo;
            LoadAutomezzoData();
            // Aggiorna il titolo del form in base alla modalità (nuovo/modifica)
            this.Text = _currentAutomezzo.Id == 0 ? "Nuovo Automezzo" : "Modifica Automezzo";
            await LoadConducentiData();
            await LoadRimorchiData();
        }

        /// <summary>
        /// Carica i dati dell'automezzo nei controlli del form.
        /// </summary>
        private void LoadAutomezzoData()
        {
            if (_currentAutomezzo != null)
            {
                txtDescrizione.Text = _currentAutomezzo.Descrizione;
                txtTarga.Text = _currentAutomezzo.Targa;
            }
            else
            {
                txtDescrizione.Text = string.Empty;
                txtTarga.Text = string.Empty;
            }
        }

        /// <summary>
        /// Carica tutti i conducenti per la ricerca e quelli già associati nella lista.
        /// </summary>
        private async Task LoadConducentiData()
        {
            // 1. Carica tutti i conducenti nella ComboBox di ricerca
            _allConducenti = (await _conducenteRepository.GetAllAsync()).OrderBy(c => c.Descrizione).ToList();
            
            cmbConducentiResult.DataSource = _allConducenti; // Inizialmente mostra tutti
            cmbConducentiResult.DisplayMember = "DisplayText"; // Usa la nuova proprietà per la visualizzazione
            cmbConducentiResult.ValueMember = "Id";
            cmbConducentiResult.SelectedIndex = -1;

            // 2. Carica i conducenti già associati nella ListBox
            lstConducentiAssociati.Items.Clear();
            lstConducentiAssociati.DisplayMember = "DisplayText"; // Usa la nuova proprietà anche qui
            if (_currentAutomezzo != null && _currentAutomezzo.Id != 0)
            {
                var associatedConducentiIds = (await _automCondRepository.FindAsync(ac => ac.Id_Automezzo == _currentAutomezzo.Id))
                                                .Select(ac => ac.Id_Conducente)
                                                .ToHashSet();

                var associatedConducenti = _allConducenti.Where(c => associatedConducentiIds.Contains(c.Id));
                foreach (var conducente in associatedConducenti)
                {
                    lstConducentiAssociati.Items.Add(conducente);
                }
            }
        }

        /// <summary>
        /// Carica tutti i rimorchi per la ricerca e quelli già associati nella lista.
        /// </summary>
        private async Task LoadRimorchiData()
        {
            // 1. Carica tutti i rimorchi nella ComboBox di ricerca
            _allRimorchi = (await _rimorchioRepository.GetAllAsync()).OrderBy(r => r.Descrizione).ToList();

            cmbRimorchiResult.DataSource = _allRimorchi;
            cmbRimorchiResult.DisplayMember = "Descrizione";
            cmbRimorchiResult.ValueMember = "Id";
            cmbRimorchiResult.SelectedIndex = -1;

            // 2. Carica i rimorchi già associati nella ListBox
            lstRimorchiAssociati.Items.Clear();
            lstRimorchiAssociati.DisplayMember = "Descrizione";
            if (_currentAutomezzo != null && _currentAutomezzo.Id != 0)
            {
                var associatedRimorchiIds = (await _automRimRepository.FindAsync(ar => ar.Id_Automezzo == _currentAutomezzo.Id))
                                                .Select(ar => ar.Id_Rimorchio)
                                                .ToHashSet();

                var associatedRimorchi = _allRimorchi.Where(r => associatedRimorchiIds.Contains(r.Id));
                foreach (var rimorchio in associatedRimorchi)
                {
                    lstRimorchiAssociati.Items.Add(rimorchio);
                }
            }
        }

        /// <summary>
        /// Filtra la lista dei conducenti mentre l'utente digita (autocompletamento manuale).
        /// </summary>
        private void TxtConducenteSearch_TextChanged(object? sender, EventArgs e)
        {
            // Ad ogni pressione di un tasto, riavvia il timer.
            // La ricerca vera e propria avverrà solo quando l'utente smette di digitare.
            _conducenteSearchDebounceTimer.Stop();
            _conducenteSearchDebounceTimer.Start();
        }

        private void TxtRimorchioSearch_TextChanged(object? sender, EventArgs e)
        {
            _rimorchioSearchDebounceTimer.Stop();
            _rimorchioSearchDebounceTimer.Start();
        }

        /// <summary>
        private void PerformConducenteFiltering(object? sender, EventArgs e)
        {
            _conducenteSearchDebounceTimer.Stop(); // Il timer ha fatto il suo dovere, lo fermiamo.

            string filterText = txtConducenteSearch.Text;

            List<Conducente> filteredList;
            if (string.IsNullOrWhiteSpace(filterText))
            {
                filteredList = _allConducenti;
            }
            else
            {
                filteredList = _allConducenti
                    .Where(c => c.Descrizione.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(c => c.Descrizione) // Assicura che anche la lista filtrata sia ordinata
                    .ToList();
            }

            bool searchFailed = !filteredList.Any() && !string.IsNullOrWhiteSpace(filterText);

            if (searchFailed)
            {
                // Se la ricerca non ha prodotto risultati, ripopoliamo la lista con tutti gli elementi
                // per evitare di assegnare una lista vuota che causa l'errore.
                filteredList = _allConducenti;
            }

            // Per evitare problemi di binding, specialmente con liste vuote,
            // è più sicuro annullare il binding prima di riassegnarlo.
            cmbConducentiResult.DataSource = null;
            cmbConducentiResult.DataSource = filteredList;
            cmbConducentiResult.DisplayMember = "DisplayText"; // È necessario reimpostare DisplayMember...
            cmbConducentiResult.ValueMember = "Id";             // ...e anche ValueMember per mantenere il binding coerente.
            cmbConducentiResult.SelectedIndex = -1;

            // Gestisce la visibilità del dropdown e il focus
            if (searchFailed)
            {
                cmbConducentiResult.DroppedDown = false;
                txtConducenteSearch.Focus();
                txtConducenteSearch.SelectAll();
            }
            else if (txtConducenteSearch.Focused && !string.IsNullOrWhiteSpace(filterText))
            {
                cmbConducentiResult.DroppedDown = true;
            }
            else
            {
                cmbConducentiResult.DroppedDown = false;
            }
        }

        /// <summary>
        private void PerformRimorchioFiltering(object? sender, EventArgs e)
        {
            _rimorchioSearchDebounceTimer.Stop();

            string filterText = txtRimorchioSearch.Text;

            List<Rimorchio> filteredList;
            if (string.IsNullOrWhiteSpace(filterText))
            {
                filteredList = _allRimorchi;
            }
            else
            {
                filteredList = _allRimorchi
                    .Where(r => r.Descrizione.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(r => r.Descrizione) // Assicura che anche la lista filtrata sia ordinata
                    .ToList();
            }

            bool searchFailed = !filteredList.Any() && !string.IsNullOrWhiteSpace(filterText);

            if (searchFailed)
            {
                // Se la ricerca non ha prodotto risultati, ripopoliamo la lista con tutti gli elementi.
                filteredList = _allRimorchi;
            }

            // Per evitare problemi di binding, specialmente con liste vuote,
            // è più sicuro annullare il binding prima di riassegnarlo.
            cmbRimorchiResult.DataSource = null;
            cmbRimorchiResult.DataSource = filteredList;
            cmbRimorchiResult.DisplayMember = "Descrizione"; // È necessario reimpostare DisplayMember...
            cmbRimorchiResult.ValueMember = "Id";            // ...e anche ValueMember per mantenere il binding coerente.
            cmbRimorchiResult.SelectedIndex = -1;

            // Gestisce la visibilità del dropdown e il focus
            if (searchFailed)
            {
                cmbRimorchiResult.DroppedDown = false;
                txtRimorchioSearch.Focus();
                txtRimorchioSearch.SelectAll();
            }
            else if (txtRimorchioSearch.Focused && !string.IsNullOrWhiteSpace(filterText))
            {
                cmbRimorchiResult.DroppedDown = true;
            }
            else
            {
                cmbRimorchiResult.DroppedDown = false;
            }
        }

        /// <summary>
        /// Gestisce la pressione del tasto Invio nel campo di ricerca dei conducenti.
        /// Se la lista dei risultati è aperta, sposta il focus su di essa.
        /// </summary>
        private void TxtConducenteSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                // Se la dropdown è aperta, significa che ci sono risultati da navigare
                if (cmbConducentiResult.DroppedDown)
                {
                    cmbConducentiResult.Focus();
                    // Se ci sono elementi nella lista, seleziona il primo per permettere la navigazione immediata con le frecce.
                    if (cmbConducentiResult.Items.Count > 0)
                    {
                        cmbConducentiResult.SelectedIndex = 0;
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Gestisce la pressione del tasto Invio nel campo di ricerca dei rimorchi.
        /// Se la lista dei risultati è aperta, sposta il focus su di essa.
        /// </summary>
        private void TxtRimorchioSearch_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                // Se la dropdown è aperta, significa che ci sono risultati da navigare
                if (cmbRimorchiResult.DroppedDown)
                {
                    cmbRimorchiResult.Focus();
                    // Se ci sono elementi nella lista, seleziona il primo per permettere la navigazione immediata con le frecce.
                    if (cmbRimorchiResult.Items.Count > 0)
                    {
                        cmbRimorchiResult.SelectedIndex = 0;
                    }
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Aggiunge il conducente selezionato alla lista degli associati, se non già presente.
        /// </summary>
        private void btnAggiungiConducente_Click(object? sender, EventArgs e)
        {
            if (cmbConducentiResult.SelectedItem is Conducente selectedConducente)
            {
                bool alreadyExists = lstConducentiAssociati.Items
                                        .OfType<Conducente>()
                                        .Any(c => c.Id == selectedConducente.Id);

                if (!alreadyExists)
                {
                    lstConducentiAssociati.Items.Add(selectedConducente);

                    // Pulisce la ricerca. Questo attiverà il filtro che ripristinerà la combo.
                    txtConducenteSearch.Clear();
                    txtConducenteSearch.Focus();
                }
                else
                {
                    MessageBox.Show("Il conducente è già associato a questo automezzo.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Gestisce la pressione del tasto Invio nella ComboBox di ricerca.
        /// Se un elemento è selezionato, sposta il focus sul pulsante "Aggiungi".
        /// </summary>
        private void CmbConducentiResult_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (cmbConducentiResult.SelectedItem != null)
                {
                    // Sposta il focus sul pulsante "Aggiungi"
                    btnAggiungiConducente.Focus();
                    
                    // Sopprime il suono di "ding" di Windows e impedisce ulteriori elaborazioni dell'evento
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        private void CmbRimorchiResult_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (cmbRimorchiResult.SelectedItem != null)
                {
                    btnAggiungiRimorchio.Focus();
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Rimuove il conducente selezionato dalla lista degli associati.
        /// </summary>
        private void btnRimuoviConducente_Click(object? sender, EventArgs e)
        {
            if (lstConducentiAssociati.SelectedItem != null)
            {
                lstConducentiAssociati.Items.Remove(lstConducentiAssociati.SelectedItem);
            }
            else
            {
                MessageBox.Show("Selezionare un conducente da rimuovere dalla lista.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnAggiungiRimorchio_Click(object? sender, EventArgs e)
        {
            if (cmbRimorchiResult.SelectedItem is Rimorchio selectedRimorchio)
            {
                bool alreadyExists = lstRimorchiAssociati.Items
                                        .OfType<Rimorchio>()
                                        .Any(r => r.Id == selectedRimorchio.Id);

                if (!alreadyExists)
                {
                    lstRimorchiAssociati.Items.Add(selectedRimorchio);

                    // Pulisce la ricerca. Questo attiverà il filtro che ripristinerà la combo.
                    txtRimorchioSearch.Clear();
                    txtRimorchioSearch.Focus();
                }
                else
                {
                    MessageBox.Show("Il rimorchio è già associato a questo automezzo.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void btnRimuoviRimorchio_Click(object? sender, EventArgs e)
        {
            if (lstRimorchiAssociati.SelectedItem != null)
            {
                lstRimorchiAssociati.Items.Remove(lstRimorchiAssociati.SelectedItem);
            }
            else
            {
                MessageBox.Show("Selezionare un rimorchio da rimuovere dalla lista.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
        private async void btnSalvaClick(object? sender, EventArgs e) // Aggiunto ? per nullable
        {
            if (!ValidateInput())
            {
                return;
            }

            if (_currentAutomezzo == null)
            {
                _currentAutomezzo = new Automezzo();
            }

            _currentAutomezzo.Descrizione = txtDescrizione.Text.Trim();
            _currentAutomezzo.Targa = txtTarga.Text.Trim();

            try
            {
                if (_currentAutomezzo.Id == 0) // Nuovo automezzo
                {
                    await _automezzoRepository.AddAsync(_currentAutomezzo);
                }
                else // Automezzo esistente
                {
                    _automezzoRepository.Update(_currentAutomezzo);
                }
                await _automezzoRepository.SaveChangesAsync();

                // Gestione delle associazioni Conducente-Automezzo
                if (_currentAutomezzo.Id != 0)
                {
                    // Approccio più efficiente: Sincronizza le associazioni invece di cancellare e ricreare.

                    // 1. Ottieni gli ID dei conducenti attualmente nella lista UI.
                    var selectedConducenteIds = lstConducentiAssociati.Items
                                                             .OfType<Conducente>()
                                                             .Select(c => c.Id)
                                                             .ToHashSet();

                    // 2. Ottieni le associazioni esistenti dal database.
                    var existingAssociations = (await _automCondRepository.FindAsync(ac => ac.Id_Automezzo == _currentAutomezzo.Id)).ToList();
                    var existingConducenteIds = existingAssociations.Select(ac => ac.Id_Conducente).ToHashSet();

                    // 3. Trova e cancella le associazioni che non sono più selezionate.
                    var associationsToDelete = existingAssociations.Where(assoc => !selectedConducenteIds.Contains(assoc.Id_Conducente));
                    foreach (var association in associationsToDelete)
                    {
                        _automCondRepository.Delete(association);
                    }

                    // 4. Trova e aggiungi le nuove associazioni.
                    var conducenteIdsToAdd = selectedConducenteIds.Where(id => !existingConducenteIds.Contains(id));
                    foreach (var conducenteId in conducenteIdsToAdd)
                    {
                        await _automCondRepository.AddAsync(new Autom_Cond { Id_Automezzo = _currentAutomezzo.Id, Id_Conducente = conducenteId });
                    }
                    await _automCondRepository.SaveChangesAsync(); // Salva tutte le modifiche (delete e add) in una sola volta.
                }

                // Gestione delle associazioni Rimorchio-Automezzo
                if (_currentAutomezzo.Id != 0)
                {
                    var selectedRimorchioIds = lstRimorchiAssociati.Items
                                                             .OfType<Rimorchio>()
                                                             .Select(r => r.Id)
                                                             .ToHashSet();

                    var existingAssociations = (await _automRimRepository.FindAsync(ar => ar.Id_Automezzo == _currentAutomezzo.Id)).ToList();
                    var existingRimorchioIds = existingAssociations.Select(ar => ar.Id_Rimorchio).ToHashSet();

                    var associationsToDelete = existingAssociations.Where(assoc => !selectedRimorchioIds.Contains(assoc.Id_Rimorchio));
                    foreach (var association in associationsToDelete)
                    {
                        _automRimRepository.Delete(association);
                    }

                    var rimorchioIdsToAdd = selectedRimorchioIds.Where(id => !existingRimorchioIds.Contains(id));
                    foreach (var rimorchioId in rimorchioIdsToAdd)
                    {
                        await _automRimRepository.AddAsync(new Autom_Rim { Id_Automezzo = _currentAutomezzo.Id, Id_Rimorchio = rimorchioId });
                    }
                    await _automRimRepository.SaveChangesAsync();
                }

                MessageBox.Show("Automezzo salvato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK; // Imposta il DialogResult a OK
                this.Close(); // Chiude il form
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio dell'automezzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtDescrizione.Text))
            {
                MessageBox.Show("Descrizione è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDescrizione.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtTarga.Text))
            {
                MessageBox.Show("Targa è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTarga.Focus();
                return false;
            }
            return true;
        }

        // Se hai un pulsante Annulla, implementa il suo handler:
        private void btnAnnullaClick(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Codice generato dal designer
        #region Windows Form Designer generated code

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            // È buona norma effettuare il Dispose anche dei timer per liberare le risorse
            if (disposing)
            {
                if (_conducenteSearchDebounceTimer != null)
                {
                    _conducenteSearchDebounceTimer.Tick -= PerformConducenteFiltering;
                    _conducenteSearchDebounceTimer.Dispose();
                }
                if (_rimorchioSearchDebounceTimer != null)
                {
                    _rimorchioSearchDebounceTimer.Tick -= PerformRimorchioFiltering;
                    _rimorchioSearchDebounceTimer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblDescrizione = new Label();
            txtDescrizione = new TextBox();
            lblTarga = new Label();
            txtTarga = new TextBox();
            btnSalva = new Button();
            btnAnnulla = new Button();
            txtConducenteSearch = new TextBox();
            lblCercaConducente = new Label();
            cmbConducentiResult = new ComboBox();
            btnAggiungiConducente = new Button();
            lblConducentiAssociati = new Label();
            lstConducentiAssociati = new ListBox();
            btnRimuoviConducente = new Button();
            txtRimorchioSearch = new TextBox();
            lblCercaRimorchio = new Label();
            cmbRimorchiResult = new ComboBox();
            btnAggiungiRimorchio = new Button();
            lblRimorchiAssociati = new Label();
            lstRimorchiAssociati = new ListBox();
            btnRimuoviRimorchio = new Button();
            SuspendLayout();
            // 
            // lblDescrizione
            // 
            lblDescrizione.AutoSize = true;
            lblDescrizione.Location = new Point(22, 32);
            lblDescrizione.Margin = new Padding(6, 0, 6, 0);
            lblDescrizione.Name = "lblDescrizione";
            lblDescrizione.Size = new Size(142, 32);
            lblDescrizione.TabIndex = 0;
            lblDescrizione.Text = "Descrizione:";
            // 
            // txtDescrizione
            // 
            txtDescrizione.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDescrizione.Location = new Point(223, 26);
            txtDescrizione.Margin = new Padding(6, 6, 6, 6);
            txtDescrizione.MaxLength = 255;
            txtDescrizione.Name = "txtDescrizione";
            txtDescrizione.Size = new Size(676, 39);
            txtDescrizione.TabIndex = 1;
            // 
            // lblTarga
            // 
            lblTarga.AutoSize = true;
            lblTarga.Location = new Point(22, 94);
            lblTarga.Margin = new Padding(6, 0, 6, 0);
            lblTarga.Name = "lblTarga";
            lblTarga.Size = new Size(75, 32);
            lblTarga.TabIndex = 2;
            lblTarga.Text = "Targa:";
            // 
            // txtTarga
            // 
            txtTarga.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTarga.Location = new Point(223, 87);
            txtTarga.Margin = new Padding(6, 6, 6, 6);
            txtTarga.MaxLength = 20;
            txtTarga.Name = "txtTarga";
            txtTarga.Size = new Size(676, 39);
            txtTarga.TabIndex = 3;
            // 
            // btnSalva
            // 
            btnSalva.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSalva.Location = new Point(763, 917);
            btnSalva.Margin = new Padding(6, 6, 6, 6);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(139, 64);
            btnSalva.TabIndex = 16;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            // 
            // btnAnnulla
            // 
            btnAnnulla.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAnnulla.Location = new Point(613, 917);
            btnAnnulla.Margin = new Padding(6, 6, 6, 6);
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(139, 64);
            btnAnnulla.TabIndex = 17;
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            // 
            // txtConducenteSearch
            // 
            txtConducenteSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConducenteSearch.Location = new Point(223, 151);
            txtConducenteSearch.Margin = new Padding(6, 6, 6, 6);
            txtConducenteSearch.Name = "txtConducenteSearch";
            txtConducenteSearch.Size = new Size(526, 39);
            txtConducenteSearch.TabIndex = 5;
            // 
            // lblCercaConducente
            // 
            lblCercaConducente.AutoSize = true;
            lblCercaConducente.Location = new Point(22, 158);
            lblCercaConducente.Margin = new Padding(6, 0, 6, 0);
            lblCercaConducente.Name = "lblCercaConducente";
            lblCercaConducente.Size = new Size(215, 32);
            lblCercaConducente.TabIndex = 4;
            lblCercaConducente.Text = "Cerca Conducente:";
            // 
            // cmbConducentiResult
            // 
            cmbConducentiResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbConducentiResult.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbConducentiResult.FormattingEnabled = true;
            cmbConducentiResult.Location = new Point(223, 213);
            cmbConducentiResult.Margin = new Padding(6, 6, 6, 6);
            cmbConducentiResult.Name = "cmbConducentiResult";
            cmbConducentiResult.Size = new Size(526, 40);
            cmbConducentiResult.TabIndex = 6;
            // 
            // btnAggiungiConducente
            // 
            btnAggiungiConducente.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAggiungiConducente.Location = new Point(763, 149);
            btnAggiungiConducente.Margin = new Padding(6, 6, 6, 6);
            btnAggiungiConducente.Name = "btnAggiungiConducente";
            btnAggiungiConducente.Size = new Size(139, 53);
            btnAggiungiConducente.TabIndex = 7;
            btnAggiungiConducente.Text = "Aggiungi";
            btnAggiungiConducente.UseVisualStyleBackColor = true;
            // 
            // lblConducentiAssociati
            // 
            lblConducentiAssociati.AutoSize = true;
            lblConducentiAssociati.Location = new Point(22, 282);
            lblConducentiAssociati.Margin = new Padding(6, 0, 6, 0);
            lblConducentiAssociati.Name = "lblConducentiAssociati";
            lblConducentiAssociati.Size = new Size(111, 32);
            lblConducentiAssociati.TabIndex = 8;
            lblConducentiAssociati.Text = "Associati:";
            // 
            // lstConducentiAssociati
            // 
            lstConducentiAssociati.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstConducentiAssociati.FormattingEnabled = true;
            lstConducentiAssociati.Location = new Point(223, 275);
            lstConducentiAssociati.Margin = new Padding(6, 6, 6, 6);
            lstConducentiAssociati.Name = "lstConducentiAssociati";
            lstConducentiAssociati.Size = new Size(526, 292);
            lstConducentiAssociati.TabIndex = 8;
            // 
            // btnRimuoviConducente
            // 
            btnRimuoviConducente.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRimuoviConducente.Location = new Point(763, 275);
            btnRimuoviConducente.Margin = new Padding(6, 6, 6, 6);
            btnRimuoviConducente.Name = "btnRimuoviConducente";
            btnRimuoviConducente.Size = new Size(139, 53);
            btnRimuoviConducente.TabIndex = 9;
            btnRimuoviConducente.Text = "Rimuovi";
            btnRimuoviConducente.UseVisualStyleBackColor = true;
            // 
            // txtRimorchioSearch
            // 
            txtRimorchioSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRimorchioSearch.Location = new Point(223, 587);
            txtRimorchioSearch.Margin = new Padding(6, 6, 6, 6);
            txtRimorchioSearch.Name = "txtRimorchioSearch";
            txtRimorchioSearch.Size = new Size(526, 39);
            txtRimorchioSearch.TabIndex = 13;
            // 
            // lblCercaRimorchio
            // 
            lblCercaRimorchio.AutoSize = true;
            lblCercaRimorchio.Location = new Point(22, 593);
            lblCercaRimorchio.Margin = new Padding(6, 0, 6, 0);
            lblCercaRimorchio.Name = "lblCercaRimorchio";
            lblCercaRimorchio.Size = new Size(193, 32);
            lblCercaRimorchio.TabIndex = 12;
            lblCercaRimorchio.Text = "Cerca Rimorchio:";
            // 
            // cmbRimorchiResult
            // 
            cmbRimorchiResult.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbRimorchiResult.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRimorchiResult.FormattingEnabled = true;
            cmbRimorchiResult.Location = new Point(223, 649);
            cmbRimorchiResult.Margin = new Padding(6, 6, 6, 6);
            cmbRimorchiResult.Name = "cmbRimorchiResult";
            cmbRimorchiResult.Size = new Size(526, 40);
            cmbRimorchiResult.TabIndex = 14;
            // 
            // btnAggiungiRimorchio
            // 
            btnAggiungiRimorchio.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAggiungiRimorchio.Location = new Point(763, 585);
            btnAggiungiRimorchio.Margin = new Padding(6, 6, 6, 6);
            btnAggiungiRimorchio.Name = "btnAggiungiRimorchio";
            btnAggiungiRimorchio.Size = new Size(139, 53);
            btnAggiungiRimorchio.TabIndex = 14;
            btnAggiungiRimorchio.Text = "Aggiungi";
            btnAggiungiRimorchio.UseVisualStyleBackColor = true;
            // 
            // lblRimorchiAssociati
            // 
            lblRimorchiAssociati.AutoSize = true;
            lblRimorchiAssociati.Location = new Point(22, 717);
            lblRimorchiAssociati.Margin = new Padding(6, 0, 6, 0);
            lblRimorchiAssociati.Name = "lblRimorchiAssociati";
            lblRimorchiAssociati.Size = new Size(111, 32);
            lblRimorchiAssociati.TabIndex = 15;
            lblRimorchiAssociati.Text = "Associati:";
            // 
            // lstRimorchiAssociati
            // 
            lstRimorchiAssociati.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstRimorchiAssociati.FormattingEnabled = true;
            lstRimorchiAssociati.Location = new Point(223, 710);
            lstRimorchiAssociati.Margin = new Padding(6, 6, 6, 6);
            lstRimorchiAssociati.Name = "lstRimorchiAssociati";
            lstRimorchiAssociati.Size = new Size(526, 292);
            lstRimorchiAssociati.TabIndex = 16;
            // 
            // btnRimuoviRimorchio
            // 
            btnRimuoviRimorchio.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRimuoviRimorchio.Location = new Point(763, 710);
            btnRimuoviRimorchio.Margin = new Padding(6, 6, 6, 6);
            btnRimuoviRimorchio.Name = "btnRimuoviRimorchio";
            btnRimuoviRimorchio.Size = new Size(139, 53);
            btnRimuoviRimorchio.TabIndex = 17;
            btnRimuoviRimorchio.Text = "Rimuovi";
            btnRimuoviRimorchio.UseVisualStyleBackColor = true;
            // 
            // AutomezziDetailForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(925, 1067);
            Controls.Add(btnRimuoviRimorchio);
            Controls.Add(lstRimorchiAssociati);
            Controls.Add(lblRimorchiAssociati);
            Controls.Add(btnAggiungiRimorchio);
            Controls.Add(cmbRimorchiResult);
            Controls.Add(txtRimorchioSearch);
            Controls.Add(lblCercaRimorchio);
            Controls.Add(btnRimuoviConducente);
            Controls.Add(lstConducentiAssociati);
            Controls.Add(lblConducentiAssociati);
            Controls.Add(btnAggiungiConducente);
            Controls.Add(cmbConducentiResult);
            Controls.Add(txtConducenteSearch);
            Controls.Add(lblCercaConducente);
            Controls.Add(btnAnnulla);
            Controls.Add(btnSalva);
            Controls.Add(txtTarga);
            Controls.Add(lblTarga);
            Controls.Add(txtDescrizione);
            Controls.Add(lblDescrizione);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(6, 6, 6, 6);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AutomezziDetailForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Dettagli Automezzo";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}