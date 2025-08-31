﻿// File: Forms/RimorchiListForm.cs
// Questo form visualizza un elenco di rimorchi, permette la ricerca e le operazioni CRUD.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using Microsoft.EntityFrameworkCore;
using FormulariRif_G.Models;
using FormulariRif_G.Service;
using System.Linq;
using System.Windows.Forms;
using System;
using System.ComponentModel;

namespace FormulariRif_G.Forms
{
    public partial class RimorchiListForm : Form
    {
        private readonly IGenericRepository<Rimorchio> _rimorchioRepository;
        private readonly IGenericRepository<Configurazione> _configurazioneRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;

        // Dichiarazioni dei controlli
        private System.Windows.Forms.DataGridView dataGridViewRimorchi;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnAggiorna;
        private TextBox txtRicerca;
        private Button btnDettagli;
        private Button btElimina;
        private Button btModifica;
        private Button btNuovo;
        private Button btAggiorna;
        private Label label1;
        private System.Windows.Forms.Label lblRicerca;

        public RimorchiListForm(IGenericRepository<Rimorchio> rimorchioRepository, IGenericRepository<Configurazione> configurazioneRepository, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _rimorchioRepository = rimorchioRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;

            this.Load += RimorchiListForm_Load;
            if (btnNuovo != null) btnNuovo.Click += btnNuovo_Click;
            if (btnModifica != null) btnModifica.Click += btnModifica_Click;
            if (btnElimina != null) btnElimina.Click += btnElimina_Click;
            if (btnAggiorna != null) btnAggiorna.Click += btnAggiorna_Click;
            if (txtRicerca != null) txtRicerca.TextChanged += TxtRicerca_TextChanged;

            // Inizializza il timer per il "debouncing" della ricerca
            _searchDebounceTimer = new System.Windows.Forms.Timer();
            _searchDebounceTimer.Interval = 500; // Ritardo di 500ms
            _searchDebounceTimer.Tick += SearchDebounceTimer_Tick;
        }

        private async void RimorchiListForm_Load(object? sender, EventArgs e)
        {
            await LoadRimorchiAsync();
            txtRicerca?.Focus();
        }

        private async Task LoadRimorchiAsync()
        {
            try
            {
                var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
                bool showTestData = configurazione?.DatiTest ?? false;

                IQueryable<Rimorchio> query = _rimorchioRepository.AsQueryable();

                if (!showTestData)
                {
                    query = query.Where(r => r.IsTestData == false);
                }

                string searchTerm = txtRicerca.Text.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(r => r.Descrizione.ToLower().Contains(lowerSearchTerm) || r.Targa.ToLower().Contains(lowerSearchTerm));
                }

                var rimorchi = await query.OrderBy(r => r.Descrizione).ToListAsync();
                dataGridViewRimorchi.DataSource = rimorchi;

                if (dataGridViewRimorchi.Columns.Contains("Id"))
                {
                    dataGridViewRimorchi.Columns["Id"].Visible = false;
                }
                if (dataGridViewRimorchi.Columns.Contains("IsTestData"))
                {
                    dataGridViewRimorchi.Columns["IsTestData"].Visible = false;
                }
                if (dataGridViewRimorchi.Columns.Contains("RimorchioAutomezzi"))
                {
                    dataGridViewRimorchi.Columns["RimorchioAutomezzi"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei rimorchi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            using (var detailForm = _serviceProvider.GetRequiredService<RimorchiDetailForm>())
            {
                detailForm.SetRimorchio(new Rimorchio(), false);
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadRimorchiAsync();
                }
            }
        }

        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewRimorchi.SelectedRows.Count > 0)
            {
                var selectedRimorchio = dataGridViewRimorchi.SelectedRows[0].DataBoundItem as Rimorchio;
                if (selectedRimorchio != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<RimorchiDetailForm>())
                    {
                        detailForm.SetRimorchio(selectedRimorchio, false);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadRimorchiAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Selezionare un rimorchio da modificare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewRimorchi.SelectedRows.Count > 0)
            {
                var selectedRimorchio = dataGridViewRimorchi.SelectedRows[0].DataBoundItem as Rimorchio;
                if (selectedRimorchio != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<RimorchiDetailForm>())
                    {
                        detailForm.SetRimorchio(selectedRimorchio, true);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadRimorchiAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Selezionare un rimorchio da modificare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewRimorchi.SelectedRows.Count > 0)
            {
                var selectedRimorchio = dataGridViewRimorchi.SelectedRows[0].DataBoundItem as Rimorchio;
                if (selectedRimorchio != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il rimorchio '{selectedRimorchio.Descrizione}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _rimorchioRepository.Delete(selectedRimorchio);
                            await _rimorchioRepository.SaveChangesAsync();
                            await LoadRimorchiAsync();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Selezionare un rimorchio da eliminare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            txtRicerca.Clear();
            await LoadRimorchiAsync();
        }

        private void TxtRicerca_TextChanged(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private async void SearchDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            await LoadRimorchiAsync();
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            dataGridViewRimorchi = new DataGridView();
            btnNuovo = new Button();
            btnModifica = new Button();
            btnElimina = new Button();
            btnAggiorna = new Button();
            txtRicerca = new TextBox();
            lblRicerca = new Label();
            btnDettagli = new Button();
            btElimina = new Button();
            btModifica = new Button();
            btNuovo = new Button();
            btAggiorna = new Button();
            label1 = new Label();
            ((ISupportInitialize)dataGridViewRimorchi).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewRimorchi
            // 
            dataGridViewRimorchi.AllowUserToAddRows = false;
            dataGridViewRimorchi.AllowUserToDeleteRows = false;
            dataGridViewRimorchi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewRimorchi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewRimorchi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewRimorchi.Location = new Point(32, 114);
            dataGridViewRimorchi.MultiSelect = false;
            dataGridViewRimorchi.Name = "dataGridViewRimorchi";
            dataGridViewRimorchi.ReadOnly = true;
            //dataGridViewRimorchi.RowHeadersWidth = 82;
            //dataGridViewRimorchi.RowTemplate.Height = 25;
            dataGridViewRimorchi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewRimorchi.Size = new Size(1061, 828);
            dataGridViewRimorchi.TabIndex = 0;
            // 
            // btnNuovo
            // 
            btnNuovo.Location = new Point(0, 0);
            btnNuovo.Name = "btnNuovo";
            btnNuovo.Size = new Size(75, 23);
            btnNuovo.TabIndex = 5;
            // 
            // btnModifica
            // 
            btnModifica.Location = new Point(0, 0);
            btnModifica.Name = "btnModifica";
            btnModifica.Size = new Size(75, 23);
            btnModifica.TabIndex = 4;
            // 
            // btnElimina
            // 
            btnElimina.Location = new Point(0, 0);
            btnElimina.Name = "btnElimina";
            btnElimina.Size = new Size(75, 23);
            btnElimina.TabIndex = 3;
            // 
            // btnAggiorna
            // 
            btnAggiorna.Location = new Point(0, 0);
            btnAggiorna.Name = "btnAggiorna";
            btnAggiorna.Size = new Size(75, 23);
            btnAggiorna.TabIndex = 2;
            // 
            // txtRicerca
            // 
            txtRicerca.Location = new Point(153, 39);
            txtRicerca.Name = "txtRicerca";
            txtRicerca.Size = new Size(940, 39);
            txtRicerca.TabIndex = 1;
            // 
            // lblRicerca
            // 
            lblRicerca.Location = new Point(0, 0);
            lblRicerca.Name = "lblRicerca";
            lblRicerca.Size = new Size(100, 23);
            lblRicerca.TabIndex = 0;
            // 
            // btnDettagli
            // 
            btnDettagli.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnDettagli.Location = new Point(484, 961);
            btnDettagli.Margin = new Padding(6);
            btnDettagli.Name = "btnDettagli";
            btnDettagli.Size = new Size(139, 64);
            btnDettagli.TabIndex = 9;
            btnDettagli.Text = "Dettagli";
            btnDettagli.UseVisualStyleBackColor = true;
            btnDettagli.Click += btnDettagli_Click;
            // 
            // btElimina
            // 
            btElimina.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btElimina.Location = new Point(333, 961);
            btElimina.Margin = new Padding(6);
            btElimina.Name = "btElimina";
            btElimina.Size = new Size(139, 64);
            btElimina.TabIndex = 8;
            btElimina.Text = "Elimina";
            btElimina.UseVisualStyleBackColor = true;
            btElimina.Click += btnElimina_Click;
            // 
            // btModifica
            // 
            btModifica.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btModifica.Location = new Point(183, 961);
            btModifica.Margin = new Padding(6);
            btModifica.Name = "btModifica";
            btModifica.Size = new Size(139, 64);
            btModifica.TabIndex = 7;
            btModifica.Text = "Modifica";
            btModifica.UseVisualStyleBackColor = true;
            btModifica.Click += btnModifica_Click;
            // 
            // btNuovo
            // 
            btNuovo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btNuovo.Location = new Point(32, 961);
            btNuovo.Margin = new Padding(6);
            btNuovo.Name = "btNuovo";
            btNuovo.Size = new Size(139, 64);
            btNuovo.TabIndex = 6;
            btNuovo.Text = "Nuovo";
            btNuovo.UseVisualStyleBackColor = true;
            btNuovo.Click += btnNuovo_Click;
            // 
            // btAggiorna
            // 
            btAggiorna.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btAggiorna.Location = new Point(954, 961);
            btAggiorna.Margin = new Padding(6);
            btAggiorna.Name = "btAggiorna";
            btAggiorna.Size = new Size(139, 64);
            btAggiorna.TabIndex = 10;
            btAggiorna.Text = "Aggiorna";
            btAggiorna.UseVisualStyleBackColor = true;
            btAggiorna.Click += btnAggiorna_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(41, 46);
            label1.Margin = new Padding(6, 0, 6, 0);
            label1.Name = "label1";
            label1.Size = new Size(94, 32);
            label1.TabIndex = 11;
            label1.Text = "Ricerca:";
            // 
            // RimorchiListForm
            // 
            ClientSize = new Size(1129, 1054);
            Controls.Add(label1);
            Controls.Add(btAggiorna);
            Controls.Add(btnDettagli);
            Controls.Add(btElimina);
            Controls.Add(btModifica);
            Controls.Add(btNuovo);
            Controls.Add(lblRicerca);
            Controls.Add(txtRicerca);
            Controls.Add(btnAggiorna);
            Controls.Add(btnElimina);
            Controls.Add(btnModifica);
            Controls.Add(btnNuovo);
            Controls.Add(dataGridViewRimorchi);
            Name = "RimorchiListForm";
            Text = "Elenco Rimorchi";
            ((ISupportInitialize)dataGridViewRimorchi).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (_searchDebounceTimer != null))
            {
                _searchDebounceTimer.Tick -= SearchDebounceTimer_Tick;
                _searchDebounceTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        
    }
}