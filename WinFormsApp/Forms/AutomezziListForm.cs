// File: Forms/AutomezziListForm.cs
// Questo form visualizza un elenco di automezzi e permette le operazioni CRUD.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks; // Necessario per Task e async/await
// using Microsoft.EntityFrameworkCore; // Non strettamente necessario qui a meno di .Include() su relazioni complesse
using System.ComponentModel;
using Microsoft.EntityFrameworkCore; // Necessario per BindingList

namespace FormulariRif_G.Forms
{

    public partial class AutomezziListForm : Form
    {
        private readonly IGenericRepository<Automezzo> _automezzoRepository;        
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private readonly IGenericRepository<Autom_Cond> _automCondRepository;
        private readonly IGenericRepository<Rimorchio> _rimorchioRepository;
        private readonly IGenericRepository<Autom_Rim> _automRimRepository;
        
        private readonly IGenericRepository<Configurazione> _configurazioneRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;

        // Dichiarazioni dei controlli (corrispondenti al tuo Designer.cs)
        private System.Windows.Forms.DataGridView dataGridViewAutomezzi;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private Label lblRicerca;
        private TextBox txtRicerca;
        private System.Windows.Forms.Button btnAggiorna;

        public AutomezziListForm(IGenericRepository<Automezzo> automezzoRepository, IGenericRepository<Conducente> conducenteRepository, IGenericRepository<Autom_Cond> automCondRepository, IGenericRepository<Rimorchio> rimorchioRepository, IGenericRepository<Autom_Rim> automRimRepository, IGenericRepository<Configurazione> configurazioneRepository, IServiceProvider serviceProvider)
        
        {
            InitializeComponent();
            _automezzoRepository = automezzoRepository;
            _conducenteRepository = conducenteRepository;
            _automCondRepository = automCondRepository;
            _rimorchioRepository = rimorchioRepository;
            _automRimRepository = automRimRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;

            // Collega gli handler degli eventi ai pulsanti e al form Load.
            this.Load += AutomezziListForm_Load;
            if (btnNuovo != null) btnNuovo.Click += btnNuovo_Click;
            if (btnModifica != null) btnModifica.Click += btnModifica_Click;
            if (btnElimina != null) btnElimina.Click += btnElimina_Click;
            if (btnAggiorna != null) btnAggiorna.Click += btnAggiorna_Click;
            if (txtRicerca != null) txtRicerca.TextChanged += TxtRicerca_TextChanged;

            // Inizializza il timer per il "debouncing" della ricerca
            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = 500; // Ritardo di 500ms prima di avviare la ricerca
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;
        }

        private async void AutomezziListForm_Load(object? sender, EventArgs e)
        {
            await LoadAutomezziAsync();
            txtRicerca?.Focus();
        }

        /// <summary>
        /// Carica gli automezzi nella DataGridView.
        /// </summary>
        private async Task LoadAutomezziAsync()
        {
            //try
            //{
            //    var automezzi = await _automezzoRepository.GetAllAsync();
            //    // Assicurati che il DataSource sia impostato correttamente
            //    dataGridViewAutomezzi.DataSource = new BindingList<Automezzo>(automezzi.ToList());

            //    // Nasconde la colonna "Id" se esiste
            //    if (dataGridViewAutomezzi.Columns.Contains("Id"))
            //    {
            //        dataGridViewAutomezzi.Columns["Id"].Visible = false;
            //    }
            //    // Puoi nascondere altre colonne qui, ad esempio:
            //    // if (dataGridViewAutomezzi.Columns.Contains("ProprietaNonMostrare"))
            //    // {
            //    //     dataGridViewAutomezzi.Columns["ProprietaNonMostrare"].Visible = false;
            //    // }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Errore durante il caricamento degli automezzi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            try
            {
                var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
                bool showTestData = configurazione?.DatiTest ?? false;

                IQueryable<Automezzo> query = _automezzoRepository.AsQueryable();

                if (!showTestData)
                {
                    query = query.Where(a => a.IsTestData == false);
                }

                // Applica il filtro di ricerca se è stato inserito del testo
                string searchTerm = txtRicerca.Text.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(a => a.Descrizione.ToLower().Contains(lowerSearchTerm) || a.Targa.ToLower().Contains(lowerSearchTerm));
                }

                var automezzi = await query.OrderBy(a => a.Descrizione).ToListAsync();

                // 3. Prepara le query per le associazioni, filtrando i dati di test se necessario
                IQueryable<Autom_Cond> automCondQuery = _automCondRepository.AsQueryable();
                IQueryable<Autom_Rim> automRimQuery = _automRimRepository.AsQueryable();

                if (!showTestData)
                {
                    var validConducenteIds = _conducenteRepository.AsQueryable().Where(c => !c.IsTestData).Select(c => c.Id);
                    automCondQuery = automCondQuery.Where(ac => validConducenteIds.Contains(ac.Id_Conducente));

                    var validRimorchioIds = _rimorchioRepository.AsQueryable().Where(r => !r.IsTestData).Select(r => r.Id);
                    automRimQuery = automRimQuery.Where(ar => validRimorchioIds.Contains(ar.Id_Rimorchio));
                }
                var allValidAutomCond = await automCondQuery.ToListAsync();
                var allValidAutomRim = await automRimQuery.ToListAsync();

                // 4. Popola le proprietà [NotMapped] per ogni automezzo
                foreach (var automezzo in automezzi)
                {
                    automezzo.NumeroConducenti = allValidAutomCond.Count(ac => ac.Id_Automezzo == automezzo.Id);
                    automezzo.NumeroRimorchi = allValidAutomRim.Count(ar => ar.Id_Automezzo == automezzo.Id);
                }
                dataGridViewAutomezzi.DataSource = automezzi;

                // Nasconde la colonna "Id" se esiste
                if (dataGridViewAutomezzi.Columns.Contains("Id"))
                {
                    dataGridViewAutomezzi.Columns["Id"].Visible = false;
                }
                if (dataGridViewAutomezzi.Columns.Contains("IsTestData"))
                {
                    dataGridViewAutomezzi.Columns["IsTestData"].Visible = false;
                }
                // Nascondi anche le proprietà di navigazione che non sono utili nella griglia
                if (dataGridViewAutomezzi.Columns.Contains("AutomezziConducenti"))
                {
                    dataGridViewAutomezzi.Columns["AutomezziConducenti"].Visible = false;
                }
                if (dataGridViewAutomezzi.Columns.Contains("AutomezziRimorchi"))
                {
                    dataGridViewAutomezzi.Columns["AutomezziRimorchi"].Visible = false;
                }
                // Puoi nascondere altre colonne qui, ad esempio:
                // if (dataGridViewAutomezzi.Columns.Contains("ProprietaNonMostrare"))
                // {
                //     dataGridViewAutomezzi.Columns["ProprietaNonMostrare"].Visible = false;
                // }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli automezzi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// </summary>
        private async void btnNuovo_Click(object? sender, EventArgs e) // Aggiunto ? per nullable
        {
            // Ottieni una nuova istanza di AutomezziDetailForm tramite il ServiceProvider
            // Questo assicura che eventuali dipendenze del DetailForm siano risolte
            using (var detailForm = _serviceProvider.GetRequiredService<AutomezziDetailForm>())
            {
                detailForm.SetAutomezzo(new Automezzo()); // Passa un nuovo oggetto Automezzo
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadAutomezziAsync(); // Ricarica la lista dopo l'aggiunta
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// </summary>
        private async void btnModifica_Click(object? sender, EventArgs e) // Aggiunto ? per nullable
        {
            if (dataGridViewAutomezzi.SelectedRows.Count > 0)
            {
                // Ora possiamo recuperare l'oggetto Automezzo direttamente
                var selectedAutomezzo = dataGridViewAutomezzi.SelectedRows[0].DataBoundItem as Automezzo;
                if (selectedAutomezzo != null)
                {
                    // Ottieni una nuova istanza di AutomezziDetailForm tramite il ServiceProvider
                    using (var detailForm = _serviceProvider.GetRequiredService<AutomezziDetailForm>())
                    {
                        detailForm.SetAutomezzo(selectedAutomezzo); // Passa l'oggetto automezzo da modificare
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadAutomezziAsync(); // Ricarica la lista dopo la modifica
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un automezzo da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina".
        /// </summary>
        private async void btnElimina_Click(object? sender, EventArgs e) // Aggiunto ? per nullable
        {
            if (dataGridViewAutomezzi.SelectedRows.Count > 0)
            {
                // Ora possiamo recuperare l'oggetto Automezzo direttamente
                var selectedAutomezzo = dataGridViewAutomezzi.SelectedRows[0].DataBoundItem as Automezzo;
                if (selectedAutomezzo != null)
                {
                    var confirmResult = MessageBox.Show(
                        $"Sei sicuro di voler eliminare l'automezzo '{selectedAutomezzo.Descrizione}' con targa '{selectedAutomezzo.Targa}'?",
                        "Conferma Eliminazione",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _automezzoRepository.Delete(selectedAutomezzo);
                            await _automezzoRepository.SaveChangesAsync();
                            await LoadAutomezziAsync(); // Ricarica la lista dopo l'eliminazione
                            MessageBox.Show("Automezzo eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione dell'automezzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un automezzo da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Aggiorna".
        /// </summary>
        private async void btnAggiorna_Click(object? sender, EventArgs e) // Aggiunto ? per nullable
        {
            await LoadAutomezziAsync();
        }

        /// <summary>
        /// Gestisce la modifica del testo nel campo di ricerca, riavviando il timer di debouncing.
        /// </summary>
        private void TxtRicerca_TextChanged(object? sender, EventArgs e)
        {
            // Ad ogni pressione di un tasto, riavvia il timer.
            // La ricerca vera e propria avverrà solo quando l'utente smette di digitare.
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        /// <summary>
        /// Eseguito quando il timer di debouncing scatta, avvia il caricamento dei dati filtrati.
        /// </summary>
        private async void SearchDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop(); // Il timer ha fatto il suo dovere, lo fermiamo.
            await LoadAutomezziAsync();
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
            // È buona norma effettuare il Dispose anche del timer per liberare le risorse
            if (disposing && (_searchDebounceTimer != null))
            {
                _searchDebounceTimer.Tick -= SearchDebounceTimer_Tick;
                _searchDebounceTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridViewAutomezzi = new DataGridView();
            btnNuovo = new Button();
            btnModifica = new Button();
            btnElimina = new Button();
            btnAggiorna = new Button();
            lblRicerca = new Label();
            txtRicerca = new TextBox();
            ((ISupportInitialize)dataGridViewAutomezzi).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewAutomezzi
            // 
            dataGridViewAutomezzi.AllowUserToAddRows = false;
            dataGridViewAutomezzi.AllowUserToDeleteRows = false;
            dataGridViewAutomezzi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewAutomezzi.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewAutomezzi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewAutomezzi.Location = new Point(22, 103);
            dataGridViewAutomezzi.Margin = new Padding(6, 6, 6, 6);
            dataGridViewAutomezzi.MultiSelect = false;
            dataGridViewAutomezzi.Name = "dataGridViewAutomezzi";
            dataGridViewAutomezzi.ReadOnly = true;
            dataGridViewAutomezzi.RowHeadersWidth = 82;
            dataGridViewAutomezzi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewAutomezzi.Size = new Size(1411, 776);
            dataGridViewAutomezzi.TabIndex = 0;
            // 
            // btnNuovo
            // 
            btnNuovo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNuovo.Location = new Point(22, 896);
            btnNuovo.Margin = new Padding(6, 6, 6, 6);
            btnNuovo.Name = "btnNuovo";
            btnNuovo.Size = new Size(139, 64);
            btnNuovo.TabIndex = 1;
            btnNuovo.Text = "Nuovo";
            btnNuovo.UseVisualStyleBackColor = true;
            // 
            // btnModifica
            // 
            btnModifica.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnModifica.Location = new Point(173, 896);
            btnModifica.Margin = new Padding(6, 6, 6, 6);
            btnModifica.Name = "btnModifica";
            btnModifica.Size = new Size(139, 64);
            btnModifica.TabIndex = 2;
            btnModifica.Text = "Modifica";
            btnModifica.UseVisualStyleBackColor = true;
            // 
            // btnElimina
            // 
            btnElimina.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnElimina.Location = new Point(323, 896);
            btnElimina.Margin = new Padding(6, 6, 6, 6);
            btnElimina.Name = "btnElimina";
            btnElimina.Size = new Size(139, 64);
            btnElimina.TabIndex = 3;
            btnElimina.Text = "Elimina";
            btnElimina.UseVisualStyleBackColor = true;
            // 
            // btnAggiorna
            // 
            btnAggiorna.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAggiorna.Location = new Point(1294, 896);
            btnAggiorna.Margin = new Padding(6, 6, 6, 6);
            btnAggiorna.Name = "btnAggiorna";
            btnAggiorna.Size = new Size(139, 64);
            btnAggiorna.TabIndex = 4;
            btnAggiorna.Text = "Aggiorna";
            btnAggiorna.UseVisualStyleBackColor = true;
            // 
            // lblRicerca
            // 
            lblRicerca.AutoSize = true;
            lblRicerca.Location = new Point(25, 39);
            lblRicerca.Margin = new Padding(6, 0, 6, 0);
            lblRicerca.Name = "lblRicerca";
            lblRicerca.Size = new Size(94, 32);
            lblRicerca.TabIndex = 9;
            lblRicerca.Text = "Ricerca:";
            // 
            // txtRicerca
            // 
            txtRicerca.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRicerca.Location = new Point(133, 33);
            txtRicerca.Margin = new Padding(6);
            txtRicerca.Name = "txtRicerca";
            txtRicerca.Size = new Size(1300, 39);
            txtRicerca.TabIndex = 8;
            // 
            // AutomezziListForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1456, 983);
            Controls.Add(lblRicerca);
            Controls.Add(txtRicerca);
            Controls.Add(btnAggiorna);
            Controls.Add(btnElimina);
            Controls.Add(btnModifica);
            Controls.Add(btnNuovo);
            Controls.Add(dataGridViewAutomezzi);
            Margin = new Padding(6, 6, 6, 6);
            MinimumSize = new Size(1463, 986);
            Name = "AutomezziListForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestione Automezzi";
            ((ISupportInitialize)dataGridViewAutomezzi).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}