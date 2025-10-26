using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Linq;

namespace FormulariRif_G.Forms
{
    public partial class DestinazioniRDListForm : Form
    {
        private readonly IGenericRepository<DestinatarioDest> _destinazioniRDRepository;
        private readonly IGenericRepository<Configurazione> _configurazioneRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly FormManager _formManager;

        private readonly System.Windows.Forms.Timer _searchDebounceTimer;
        private CancellationTokenSource _cancellationTokenSource;

        public DestinazioniRDListForm(IGenericRepository<DestinatarioDest> destinazioniRDRepository,
                                  IGenericRepository<Configurazione> configurazioneRepository,
                                  IServiceProvider serviceProvider,
                                  FormManager formManager)
        {
            InitializeComponent();
            _destinazioniRDRepository = destinazioniRDRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;
            _formManager = formManager;

            this.Load += DestinazioniRDListForm_Load;
            this.FormClosed += DestinazioniRDListForm_FormClosed;
            this.dataGridViewDestinatario.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewDestinazioniRD_CellFormatting);

            // Inizializza il timer per il "debouncing" della ricerca
            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = 500;
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;
            _searchDebounceTimer.Stop();
        }

        private async void DestinazioniRDListForm_Load(object? sender, EventArgs e)
        {
            // Popola la ComboBox di filtro prima di collegare l'evento per evitare un caricamento doppio all'avvio.
            PopulateTipoFiltro();
            // Ora che la ComboBox è popolata, possiamo collegare l'evento per le interazioni future dell'utente.
            if (this.cmbTipoFiltro != null)            
                this.cmbTipoFiltro.SelectedIndexChanged += CmbTipoFiltro_SelectedIndexChanged;            

            await LoadDestinazioniRDAsync();            
            txtRicerca?.Focus();
        }

        private void DestinazioniRDListForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // Cancella e disattiva il CancellationTokenSource quando il form viene chiuso
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            // Imposta a null per evitare usi successivi di un oggetto disposto
            _cancellationTokenSource = null;
        }

        /// <summary>
        /// Carica i dati dei conducenti nella DataGridView.
        /// </summary>
        private async Task LoadDestinazioniRDAsync()
        {
            // Inizializza un nuovo CancellationTokenSource per questa operazione
            // Annulla qualsiasi operazione precedente in corso
            _cancellationTokenSource?.Cancel();
            // Rilascia le risorse del vecchio CancellationTokenSource
            _cancellationTokenSource?.Dispose();
            // Crea un nuovo CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource();
            // Ottieni il token per questa operazione
            var cancellationToken = _cancellationTokenSource.Token;

            try
            {
                var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
                bool showTestData = configurazione?.DatiTest ?? false;

                // Utilizziamo IQueryable per costruire la query in modo dinamico ed efficiente
                IQueryable<DestinatarioDest> query = _destinazioniRDRepository.AsQueryable();                
                
                if (cmbTipoFiltro.SelectedValue is int tipoValue)                
                    query = query.Where(c => c.Tipo == tipoValue);
                
                var searchText = txtRicerca?.Text.Trim() ?? "";
                if (!string.IsNullOrEmpty(searchText))
                {
                    string lowerSearchText = searchText.ToLower();
                    query = query.Where(c =>
                        c.Desc.ToLower().Contains(lowerSearchText));
                }

                // Esegui la query e popola la griglia
                var dest = await query.OrderBy(c => c.Desc).ToListAsync();
                dataGridViewDestinatario.DataSource = dest;
                if (dataGridViewDestinatario.Columns["Id"] != null) dataGridViewDestinatario.Columns["Id"].Visible = false;                                
                if (dataGridViewDestinatario.Columns.Contains("DisplayText")) dataGridViewDestinatario.Columns["DisplayText"].Visible = false;   
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento delle destinazioni: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce l'evento CellFormatting della DataGridView per visualizzare il testo corretto per il campo Tipo.
        /// </summary>
        private void dataGridViewDestinazioniRD_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewDestinatario.Columns[e.ColumnIndex].Name == "Tipo" && e.Value != null)
            {
                if (e.Value is int tipoValue)
                {
                    e.Value = tipoValue == 0 ? "R" : "D";
                    e.FormattingApplied = true;
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// Apre ConducentiDetailForm in modalità non modale per l'inserimento.
        /// </summary>
        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            var detailForm = _formManager.ShowOrActivate<DestinazioniRDDetailForm>();
            detailForm.SetConducente(new DestinatarioDest(), false);
            detailForm.FormClosed -= DetailForm_FormClosed;
            detailForm.FormClosed += DetailForm_FormClosed;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// Abre ConducentiDetailForm in modalità non modale per la modifica.
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewDestinatario.SelectedRows.Count > 0)
            {
                var selectedDest = dataGridViewDestinatario.SelectedRows[0].DataBoundItem as DestinatarioDest;
                if (selectedDest != null)
                {
                    var detailForm = _formManager.ShowOrActivate<DestinazioniRDDetailForm>();
                    detailForm.SetConducente(selectedDest, isReadOnly: false);
                    detailForm.FormClosed -= DetailForm_FormClosed;
                    detailForm.FormClosed += DetailForm_FormClosed;
                }
            }
            else
            {
                MessageBox.Show("Seleziona una destinazione da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Dettagli".
        /// Apre ConducentiDetailForm in modalità non modale di sola lettura.
        /// </summary>
        private void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewDestinatario.SelectedRows.Count > 0)
            {
                var selectedDest = dataGridViewDestinatario.SelectedRows[0].DataBoundItem as DestinatarioDest;
                if (selectedDest != null)
                {
                    var detailForm = _formManager.ShowOrActivate<DestinazioniRDDetailForm>();
                    detailForm.SetConducente(selectedDest, isReadOnly: true);
                    detailForm.FormClosed -= DetailForm_FormClosed;
                    detailForm.FormClosed += DetailForm_FormClosed;
                }
            }
            else
            {
                MessageBox.Show("Seleziona una destinazione per visualizzare i dettagli.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestore comune per l'evento FormClosed delle form di dettaglio.
        /// Ricarica i dati quando una form di dettaglio viene chiusa.
        /// </summary>
        private async void DetailForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            await LoadDestinazioniRDAsync();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina".
        /// </summary>
        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewDestinatario.SelectedRows.Count > 0)
            {
                var selectedDest = dataGridViewDestinatario.SelectedRows[0].DataBoundItem as DestinatarioDest;
                if (selectedDest != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare la destinazione '{selectedDest.Desc}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _destinazioniRDRepository.Delete(selectedDest);
                            await _destinazioniRDRepository.SaveChangesAsync();
                            await LoadDestinazioniRDAsync();
                            MessageBox.Show("Destinazione eliminata con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione della destinazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona una destinazione da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Aggiorna".
        /// </summary>
        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            await LoadDestinazioniRDAsync();
            txtRicerca.Clear();
        }

        /// <summary>
        /// Popola la ComboBox per il filtro del tipo di conducente.
        /// </summary>
        private void PopulateTipoFiltro()
        {
            var filtroItems = new Dictionary<string, int?>
            {
                { "Tutti i Tipi", null }, // Opzione per non filtrare
                { "R", 0 },
                { "D", 1 }
            };

            cmbTipoFiltro.DataSource = new BindingSource(filtroItems, null);
            cmbTipoFiltro.DisplayMember = "Key";
            cmbTipoFiltro.ValueMember = "Value";
        }

        private async void CmbTipoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            await LoadDestinazioniRDAsync();
        }
        /// <summary>
        /// Gestisce il cambio di testo nel campo di ricerca.
        /// </summary>
        private void txtRicerca_TextChanged(object sender, EventArgs e)
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
            await LoadDestinazioniRDAsync();
        }

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
            dataGridViewDestinatario = new DataGridView();
            btnNuovo = new Button();
            btnModifica = new Button();
            btnElimina = new Button();
            btnDettagli = new Button();
            btnAggiorna = new Button();
            txtRicerca = new TextBox();
            lblRicerca = new Label();
            lblTipoFiltro = new Label();
            cmbTipoFiltro = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)dataGridViewDestinatario).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewDestinatario
            // 
            dataGridViewDestinatario.AllowUserToAddRows = false;
            dataGridViewDestinatario.AllowUserToDeleteRows = false;
            dataGridViewDestinatario.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewDestinatario.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewDestinatario.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewDestinatario.Location = new Point(22, 128);
            dataGridViewDestinatario.Margin = new Padding(6);
            dataGridViewDestinatario.MultiSelect = false;
            dataGridViewDestinatario.Name = "dataGridViewDestinatario";
            dataGridViewDestinatario.ReadOnly = true;
            dataGridViewDestinatario.RowHeadersWidth = 82;
            dataGridViewDestinatario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDestinatario.Size = new Size(1411, 853);
            dataGridViewDestinatario.TabIndex = 0;
            // 
            // btnNuovo
            // 
            btnNuovo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNuovo.Location = new Point(22, 1003);
            btnNuovo.Margin = new Padding(6);
            btnNuovo.Name = "btnNuovo";
            btnNuovo.Size = new Size(139, 64);
            btnNuovo.TabIndex = 1;
            btnNuovo.Text = "Nuovo";
            btnNuovo.UseVisualStyleBackColor = true;
            btnNuovo.Click += btnNuovo_Click;
            // 
            // btnModifica
            // 
            btnModifica.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnModifica.Location = new Point(173, 1003);
            btnModifica.Margin = new Padding(6);
            btnModifica.Name = "btnModifica";
            btnModifica.Size = new Size(139, 64);
            btnModifica.TabIndex = 2;
            btnModifica.Text = "Modifica";
            btnModifica.UseVisualStyleBackColor = true;
            btnModifica.Click += btnModifica_Click;
            // 
            // btnElimina
            // 
            btnElimina.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnElimina.Location = new Point(323, 1003);
            btnElimina.Margin = new Padding(6);
            btnElimina.Name = "btnElimina";
            btnElimina.Size = new Size(139, 64);
            btnElimina.TabIndex = 3;
            btnElimina.Text = "Elimina";
            btnElimina.UseVisualStyleBackColor = true;
            btnElimina.Click += btnElimina_Click;
            // 
            // btnDettagli
            // 
            btnDettagli.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDettagli.Location = new Point(474, 1003);
            btnDettagli.Margin = new Padding(6);
            btnDettagli.Name = "btnDettagli";
            btnDettagli.Size = new Size(139, 64);
            btnDettagli.TabIndex = 4;
            btnDettagli.Text = "Dettagli";
            btnDettagli.UseVisualStyleBackColor = true;
            btnDettagli.Click += btnDettagli_Click;
            // 
            // btnAggiorna
            // 
            btnAggiorna.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAggiorna.Location = new Point(1294, 1003);
            btnAggiorna.Margin = new Padding(6);
            btnAggiorna.Name = "btnAggiorna";
            btnAggiorna.Size = new Size(139, 64);
            btnAggiorna.TabIndex = 5;
            btnAggiorna.Text = "Aggiorna";
            btnAggiorna.UseVisualStyleBackColor = true;
            btnAggiorna.Click += btnAggiorna_Click;
            // 
            // txtRicerca
            // 
            txtRicerca.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRicerca.Location = new Point(130, 47);
            txtRicerca.Margin = new Padding(6);
            txtRicerca.Name = "txtRicerca";
            txtRicerca.Size = new Size(832, 39);
            txtRicerca.TabIndex = 6;
            txtRicerca.TextChanged += txtRicerca_TextChanged;
            // 
            // lblRicerca
            // 
            lblRicerca.AutoSize = true;
            lblRicerca.Location = new Point(22, 53);
            lblRicerca.Margin = new Padding(6, 0, 6, 0);
            lblRicerca.Name = "lblRicerca";
            lblRicerca.Size = new Size(94, 32);
            lblRicerca.TabIndex = 7;
            lblRicerca.Text = "Ricerca:";
            // 
            // lblTipoFiltro
            // 
            lblTipoFiltro.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTipoFiltro.AutoSize = true;
            lblTipoFiltro.Location = new Point(984, 53);
            lblTipoFiltro.Margin = new Padding(6, 0, 6, 0);
            lblTipoFiltro.Name = "lblTipoFiltro";
            lblTipoFiltro.Size = new Size(66, 32);
            lblTipoFiltro.TabIndex = 8;
            lblTipoFiltro.Text = "Tipo:";
            // 
            // cmbTipoFiltro
            // 
            cmbTipoFiltro.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbTipoFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTipoFiltro.FormattingEnabled = true;
            cmbTipoFiltro.Location = new Point(1059, 47);
            cmbTipoFiltro.Margin = new Padding(6);
            cmbTipoFiltro.Name = "cmbTipoFiltro";
            cmbTipoFiltro.Size = new Size(372, 40);
            cmbTipoFiltro.TabIndex = 9;
            // 
            // DestinazioniRDListForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1456, 1090);
            Controls.Add(cmbTipoFiltro);
            Controls.Add(lblTipoFiltro);
            Controls.Add(lblRicerca);
            Controls.Add(txtRicerca);
            Controls.Add(btnAggiorna);
            Controls.Add(btnDettagli);
            Controls.Add(btnElimina);
            Controls.Add(btnModifica);
            Controls.Add(btnNuovo);
            Controls.Add(dataGridViewDestinatario);
            Margin = new Padding(6);
            MinimumSize = new Size(1463, 1093);
            Name = "DestinazioniRDListForm";
            Text = "Gestione Destinazione R/D";
            ((System.ComponentModel.ISupportInitialize)dataGridViewDestinatario).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.DataGridView dataGridViewDestinatario;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnDettagli;
        private System.Windows.Forms.Button btnAggiorna;
        private System.Windows.Forms.TextBox txtRicerca;
        private System.Windows.Forms.Label lblRicerca;
        private Label lblTipoFiltro;
        private ComboBox cmbTipoFiltro;

        #endregion
    }
}
