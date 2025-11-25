// File: Forms/TipiListForm.cs
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
    public partial class TipiListForm : Form
    {
        private readonly IGenericRepository<Tipo> _tipoRepository;
        private readonly IGenericRepository<Configurazione> _configurazioneRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly System.Windows.Forms.Timer _searchDebounceTimer;

        // Dichiarazioni dei controlli
        private System.Windows.Forms.DataGridView dataGridViewTipi;
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

        public TipiListForm(IGenericRepository<Tipo> tipoRepository, IGenericRepository<Configurazione> configurazioneRepository, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _tipoRepository = tipoRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;

            this.Load += TipiListForm_Load;
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

        private async void TipiListForm_Load(object? sender, EventArgs e)
        {
            await LoadTipiAsync();
            txtRicerca?.Focus();
        }

        private async Task LoadTipiAsync()
        {
            try
            {
                // Note: Tipo does not have IsTestData, so we skip that check or check if it exists.
                // Assuming Tipo is a simple lookup table without IsTestData based on the model file I saw.

                IQueryable<Tipo> query = _tipoRepository.AsQueryable();

                string searchTerm = txtRicerca.Text.Trim();
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    string lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(r => r.Descrizione.ToLower().Contains(lowerSearchTerm));
                }

                var tipi = await query.OrderBy(r => r.Descrizione).ToListAsync();
                dataGridViewTipi.DataSource = tipi;

                if (dataGridViewTipi.Columns.Contains("Id"))
                {
                    dataGridViewTipi.Columns["Id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei tipi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            using (var detailForm = _serviceProvider.GetRequiredService<TipiDetailForm>())
            {
                detailForm.SetTipo(new Tipo { Descrizione = string.Empty }, false);
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadTipiAsync();
                }
            }
        }

        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewTipi.SelectedRows.Count > 0)
            {
                var selectedTipo = dataGridViewTipi.SelectedRows[0].DataBoundItem as Tipo;
                if (selectedTipo != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<TipiDetailForm>())
                    {
                        detailForm.SetTipo(selectedTipo, false);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadTipiAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Selezionare un tipo da modificare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewTipi.SelectedRows.Count > 0)
            {
                var selectedTipo = dataGridViewTipi.SelectedRows[0].DataBoundItem as Tipo;
                if (selectedTipo != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<TipiDetailForm>())
                    {
                        detailForm.SetTipo(selectedTipo, true);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadTipiAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Selezionare un tipo da visualizzare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewTipi.SelectedRows.Count > 0)
            {
                var selectedTipo = dataGridViewTipi.SelectedRows[0].DataBoundItem as Tipo;
                if (selectedTipo != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il tipo '{selectedTipo.Descrizione}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _tipoRepository.Delete(selectedTipo);
                            await _tipoRepository.SaveChangesAsync();
                            await LoadTipiAsync();
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
                MessageBox.Show("Selezionare un tipo da eliminare.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            txtRicerca.Clear();
            await LoadTipiAsync();
        }

        private void TxtRicerca_TextChanged(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        }

        private async void SearchDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _searchDebounceTimer.Stop();
            await LoadTipiAsync();
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            dataGridViewTipi = new DataGridView();
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
            ((ISupportInitialize)dataGridViewTipi).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewTipi
            // 
            dataGridViewTipi.AllowUserToAddRows = false;
            dataGridViewTipi.AllowUserToDeleteRows = false;
            dataGridViewTipi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewTipi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewTipi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewTipi.Location = new Point(32, 114);
            dataGridViewTipi.MultiSelect = false;
            dataGridViewTipi.Name = "dataGridViewTipi";
            dataGridViewTipi.ReadOnly = true;
            dataGridViewTipi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewTipi.Size = new Size(1061, 828);
            dataGridViewTipi.TabIndex = 0;
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
            // TipiListForm
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
            Controls.Add(dataGridViewTipi);
            Name = "TipiListForm";
            Text = "Elenco Tipi";
            ((ISupportInitialize)dataGridViewTipi).EndInit();
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
