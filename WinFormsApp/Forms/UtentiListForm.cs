// File: Forms/UtentiListForm.cs
// Questo form visualizza un elenco di utenti, permette la ricerca e le operazioni CRUD.
// Ora include la logica per prevenire l'eliminazione dell'unico utente amministratore.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using System.Linq; // Per l'estensione Where

namespace FormulariRif_G.Forms
{
    public partial class UtentiListForm : Form
    {
        private readonly IGenericRepository<Utente> _utenteRepository;
        private readonly IGenericRepository<Configurazione> _configurazioneRepository; // Nuova dipendenza
        private readonly IServiceProvider _serviceProvider;

        public UtentiListForm(IGenericRepository<Utente> utenteRepository,
                              IGenericRepository<Configurazione> configurazioneRepository, // Aggiunta la dipendenza
                              IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _utenteRepository = utenteRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;
            this.Load += UtentiListForm_Load;
        }

        private async void UtentiListForm_Load(object? sender, EventArgs e)
        {
            await LoadUtentiAsync();
        }

        /// <summary>
        /// Carica i dati degli utenti nella DataGridView.
        /// </summary>
        private async Task LoadUtentiAsync()
        {
            try
            {
                var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
                bool showTestData = configurazione?.DatiTest ?? false; // Assumendo che gli utenti non siano dati di test

                IEnumerable<Utente> utenti;
                // Per gli utenti, non c'è un campo IsTestData, quindi li carichiamo sempre tutti
                // Se in futuro volessi distinguere utenti di test, dovresti aggiungere un campo IsTestData al modello Utente
                utenti = await _utenteRepository.GetAllAsync();

                dataGridViewUtenti.DataSource = utenti.ToList();
                dataGridViewUtenti.Columns["Id"].Visible = false;
                // Nasconde le colonne sensibili per motivi di sicurezza/visualizzazione
                if (dataGridViewUtenti.Columns.Contains("Password"))
                {
                    dataGridViewUtenti.Columns["Password"].Visible = false;
                }
                if (dataGridViewUtenti.Columns.Contains("PasswordSalt"))
                {
                    dataGridViewUtenti.Columns["PasswordSalt"].Visible = false;
                }
                if (dataGridViewUtenti.Columns.Contains("MustChangePassword"))
                {
                    dataGridViewUtenti.Columns["MustChangePassword"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli utenti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// </summary>
        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            using (var detailForm = _serviceProvider.GetRequiredService<UtentiDetailForm>())
            {
                detailForm.SetUtente(new Utente(), false ); 
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadUtentiAsync();
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewUtenti.SelectedRows.Count > 0)
            {
                var selectedUtente = dataGridViewUtenti.SelectedRows[0].DataBoundItem as Utente;
                if (selectedUtente != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<UtentiDetailForm>())
                    {
                        detailForm.SetUtente(selectedUtente, isReadOnly: false); 
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadUtentiAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un utente da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina".
        /// Implementa la logica per impedire l'eliminazione dell'unico utente amministratore.
        /// </summary>
        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewUtenti.SelectedRows.Count > 0)
            {
                var selectedUtente = dataGridViewUtenti.SelectedRows[0].DataBoundItem as Utente;
                if (selectedUtente != null)
                {
                    // Logica per impedire l'eliminazione dell'unico utente amministratore
                    if (selectedUtente.Admin == true)
                    {
                        var adminUsers = await _utenteRepository.FindAsync(u => u.Admin == true);
                        if (adminUsers.Count() == 1 && adminUsers.First().Id == selectedUtente.Id)
                        {
                            MessageBox.Show("Impossibile eliminare l'unico utente amministratore. È necessario che ci sia almeno un amministratore nel sistema.", "Errore Eliminazione Utente", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return; // Blocca l'eliminazione
                        }
                    }

                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare l'utente '{selectedUtente.NomeUtente}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _utenteRepository.Delete(selectedUtente);
                            await _utenteRepository.SaveChangesAsync();
                            await LoadUtentiAsync();
                            MessageBox.Show("Utente eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione dell'utente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un utente da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Dettagli".
        /// </summary>
        private void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewUtenti.SelectedRows.Count > 0)
            {
                var selectedUtente = dataGridViewUtenti.SelectedRows[0].DataBoundItem as Utente;
                if (selectedUtente != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<UtentiDetailForm>())
                    {
                        detailForm.SetUtente(selectedUtente, isReadOnly: true); // Passa il contesto admin
                        detailForm.ShowDialog();
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un utente per visualizzare i dettagli.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Aggiorna".
        /// </summary>
        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            await LoadUtentiAsync();
            txtRicerca.Clear();
        }

        /// <summary>
        /// Gestisce il cambio di testo nel campo di ricerca.
        /// </summary>
        private async void txtRicerca_TextChanged(object sender, EventArgs e)
        {
            try
            {
                var searchText = txtRicerca.Text.Trim();
                IEnumerable<Utente> filteredUtenti;

                if (string.IsNullOrEmpty(searchText))
                {
                    filteredUtenti = await _utenteRepository.GetAllAsync();
                }
                else
                {
                    filteredUtenti = await _utenteRepository.FindAsync(u =>
                        u.NomeUtente.Contains(searchText) ||
                        (u.Email != null && u.Email.Contains(searchText)));
                }
                dataGridViewUtenti.DataSource = filteredUtenti.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la ricerca: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Codice generato dal designer per UtentiListForm
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
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewUtenti = new System.Windows.Forms.DataGridView();
            this.btnNuovo = new System.Windows.Forms.Button();
            this.btnModifica = new System.Windows.Forms.Button();
            this.btnElimina = new System.Windows.Forms.Button();
            this.btnDettagli = new System.Windows.Forms.Button();
            this.btnAggiorna = new System.Windows.Forms.Button();
            this.txtRicerca = new System.Windows.Forms.TextBox();
            this.lblRicerca = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUtenti)).BeginInit();
            this.SuspendLayout();
            //
            // dataGridViewUtenti
            //
            this.dataGridViewUtenti.AllowUserToAddRows = false;
            this.dataGridViewUtenti.AllowUserToDeleteRows = false;
            this.dataGridViewUtenti.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewUtenti.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUtenti.Location = new System.Drawing.Point(12, 60);
            this.dataGridViewUtenti.MultiSelect = false;
            this.dataGridViewUtenti.Name = "dataGridViewUtenti";
            this.dataGridViewUtenti.ReadOnly = true;
            this.dataGridViewUtenti.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewUtenti.Size = new System.Drawing.Size(760, 400);
            this.dataGridViewUtenti.TabIndex = 0;
            //
            // btnNuovo
            //
            this.btnNuovo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNuovo.Location = new System.Drawing.Point(12, 470);
            this.btnNuovo.Name = "btnNuovo";
            this.btnNuovo.Size = new System.Drawing.Size(75, 30);
            this.btnNuovo.TabIndex = 1;
            this.btnNuovo.Text = "Nuovo";
            this.btnNuovo.UseVisualStyleBackColor = true;
            this.btnNuovo.Click += new System.EventHandler(this.btnNuovo_Click);
            //
            // btnModifica
            //
            this.btnModifica.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnModifica.Location = new System.Drawing.Point(93, 470);
            this.btnModifica.Name = "btnModifica";
            this.btnModifica.Size = new System.Drawing.Size(75, 30);
            this.btnModifica.TabIndex = 2;
            this.btnModifica.Text = "Modifica";
            this.btnModifica.UseVisualStyleBackColor = true;
            this.btnModifica.Click += new System.EventHandler(this.btnModifica_Click);
            //
            // btnElimina
            //
            this.btnElimina.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnElimina.Location = new System.Drawing.Point(174, 470);
            this.btnElimina.Name = "btnElimina";
            this.btnElimina.Size = new System.Drawing.Size(75, 30);
            this.btnElimina.TabIndex = 3;
            this.btnElimina.Text = "Elimina";
            this.btnElimina.UseVisualStyleBackColor = true;
            this.btnElimina.Click += new System.EventHandler(this.btnElimina_Click);
            //
            // btnDettagli
            //
            this.btnDettagli.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDettagli.Location = new System.Drawing.Point(255, 470);
            this.btnDettagli.Name = "btnDettagli";
            this.btnDettagli.Size = new System.Drawing.Size(75, 30);
            this.btnDettagli.TabIndex = 4;
            this.btnDettagli.Text = "Dettagli";
            this.btnDettagli.UseVisualStyleBackColor = true;
            this.btnDettagli.Click += new System.EventHandler(this.btnDettagli_Click);
            //
            // btnAggiorna
            //
            this.btnAggiorna.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAggiorna.Location = new System.Drawing.Point(697, 470);
            this.btnAggiorna.Name = "btnAggiorna";
            this.btnAggiorna.Size = new System.Drawing.Size(75, 30);
            this.btnAggiorna.TabIndex = 5;
            this.btnAggiorna.Text = "Aggiorna";
            this.btnAggiorna.UseVisualStyleBackColor = true;
            this.btnAggiorna.Click += new System.EventHandler(this.btnAggiorna_Click);
            //
            // txtRicerca
            //
            this.txtRicerca.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRicerca.Location = new System.Drawing.Point(70, 22);
            this.txtRicerca.Name = "txtRicerca";
            this.txtRicerca.Size = new System.Drawing.Size(702, 23);
            this.txtRicerca.TabIndex = 6;
            this.txtRicerca.TextChanged += new System.EventHandler(this.txtRicerca_TextChanged);
            //
            // lblRicerca
            //
            this.lblRicerca.AutoSize = true;
            this.lblRicerca.Location = new System.Drawing.Point(12, 25);
            this.lblRicerca.Name = "lblRicerca";
            this.lblRicerca.Size = new System.Drawing.Size(52, 15);
            this.lblRicerca.TabIndex = 7;
            this.lblRicerca.Text = "Ricerca:";
            //
            // UtentiListForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 511);
            this.Controls.Add(this.lblRicerca);
            this.Controls.Add(this.txtRicerca);
            this.Controls.Add(this.btnAggiorna);
            this.Controls.Add(this.btnDettagli);
            this.Controls.Add(this.btnElimina);
            this.Controls.Add(this.btnModifica);
            this.Controls.Add(this.btnNuovo);
            this.Controls.Add(this.dataGridViewUtenti);
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.Name = "UtentiListForm";
            this.Text = "Gestione Utenti";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUtenti)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.DataGridView dataGridViewUtenti;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnDettagli;
        private System.Windows.Forms.Button btnAggiorna;
        private System.Windows.Forms.TextBox txtRicerca;
        private System.Windows.Forms.Label lblRicerca;

        #endregion
    }
}
