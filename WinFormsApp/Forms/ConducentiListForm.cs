// File: Forms/ConducentiListForm.cs
// Questo form visualizza un elenco di conducenti, permette la ricerca e le operazioni CRUD.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using FormulariRif_G.Service;
using System.Linq;

namespace FormulariRif_G.Forms
{
    public partial class ConducentiListForm : Form
    {
        private readonly IGenericRepository<Conducente> _conducenteRepository;
        private readonly FormManager _formManager;

        public ConducentiListForm(IGenericRepository<Conducente> conducenteRepository, FormManager formManager)
        {
            InitializeComponent();
            _conducenteRepository = conducenteRepository;
            _formManager = formManager;
            this.Load += ConducentiListForm_Load;
            this.dataGridViewConducenti.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridViewConducenti_CellFormatting);
        }

        private async void ConducentiListForm_Load(object? sender, EventArgs e)
        {
            await LoadConducentiAsync();
        }

        /// <summary>
        /// Carica i dati dei conducenti nella DataGridView.
        /// </summary>
        private async Task LoadConducentiAsync()
        {
            try
            {
                IEnumerable<Conducente> conducenti;
                conducenti = await _conducenteRepository.GetAllAsync();

                dataGridViewConducenti.DataSource = conducenti.ToList();
                dataGridViewConducenti.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei conducenti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce l'evento CellFormatting della DataGridView per visualizzare il testo corretto per il campo Tipo.
        /// </summary>
        private void dataGridViewConducenti_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridViewConducenti.Columns[e.ColumnIndex].Name == "Tipo" && e.Value != null)
            {
                if (e.Value is int tipoValue)
                {
                    e.Value = tipoValue == 0 ? "Dipendente" : "Trasportatore Esterno";
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
            var detailForm = _formManager.ShowOrActivate<ConducentiDetailForm>();
            detailForm.SetConducente(new Conducente(), false);
            detailForm.FormClosed -= DetailForm_FormClosed;
            detailForm.FormClosed += DetailForm_FormClosed;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// Abre ConducentiDetailForm in modalità non modale per la modifica.
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewConducenti.SelectedRows.Count > 0)
            {
                var selectedConducente = dataGridViewConducenti.SelectedRows[0].DataBoundItem as Conducente;
                if (selectedConducente != null)
                {
                    var detailForm = _formManager.ShowOrActivate<ConducentiDetailForm>();
                    detailForm.SetConducente(selectedConducente, isReadOnly: false);
                    detailForm.FormClosed -= DetailForm_FormClosed;
                    detailForm.FormClosed += DetailForm_FormClosed;
                }
            }
            else
            {
                MessageBox.Show("Seleziona un conducente da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Dettagli".
        /// Apre ConducentiDetailForm in modalità non modale di sola lettura.
        /// </summary>
        private void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewConducenti.SelectedRows.Count > 0)
            {
                var selectedConducente = dataGridViewConducenti.SelectedRows[0].DataBoundItem as Conducente;
                if (selectedConducente != null)
                {
                    var detailForm = _formManager.ShowOrActivate<ConducentiDetailForm>();
                    detailForm.SetConducente(selectedConducente, isReadOnly: true);
                    detailForm.FormClosed -= DetailForm_FormClosed;
                    detailForm.FormClosed += DetailForm_FormClosed;
                }
            }
            else
            {
                MessageBox.Show("Seleziona un conducente per visualizzare i dettagli.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestore comune per l'evento FormClosed delle form di dettaglio.
        /// Ricarica i dati quando una form di dettaglio viene chiusa.
        /// </summary>
        private async void DetailForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            await LoadConducentiAsync();
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina".
        /// </summary>
        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewConducenti.SelectedRows.Count > 0)
            {
                var selectedConducente = dataGridViewConducenti.SelectedRows[0].DataBoundItem as Conducente;
                if (selectedConducente != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il conducente '{selectedConducente.Descrizione}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _conducenteRepository.Delete(selectedConducente);
                            await _conducenteRepository.SaveChangesAsync();
                            await LoadConducentiAsync();
                            MessageBox.Show("Conducente eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione del conducente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un conducente da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Aggiorna".
        /// </summary>
        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            await LoadConducentiAsync();
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
                IEnumerable<Conducente> filteredConducenti;

                if (string.IsNullOrEmpty(searchText))
                {
                    filteredConducenti = await _conducenteRepository.GetAllAsync();
                }
                else
                {
                    filteredConducenti = await _conducenteRepository.FindAsync(c =>
                        c.Descrizione.Contains(searchText) ||
                        (c.Contatto != null && c.Contatto.Contains(searchText)));
                }
                dataGridViewConducenti.DataSource = filteredConducenti.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la ricerca: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewConducenti = new System.Windows.Forms.DataGridView();
            this.btnNuovo = new System.Windows.Forms.Button();
            this.btnModifica = new System.Windows.Forms.Button();
            this.btnElimina = new System.Windows.Forms.Button();
            this.btnDettagli = new System.Windows.Forms.Button();
            this.btnAggiorna = new System.Windows.Forms.Button();
            this.txtRicerca = new System.Windows.Forms.TextBox();
            this.lblRicerca = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConducenti)).BeginInit();
            this.SuspendLayout();
            //
            // dataGridViewConducenti
            //
            this.dataGridViewConducenti.AllowUserToAddRows = false;
            this.dataGridViewConducenti.AllowUserToDeleteRows = false;
            this.dataGridViewConducenti.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewConducenti.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewConducenti.Location = new System.Drawing.Point(12, 60);
            this.dataGridViewConducenti.MultiSelect = false;
            this.dataGridViewConducenti.Name = "dataGridViewConducenti";
            this.dataGridViewConducenti.ReadOnly = true;
            this.dataGridViewConducenti.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewConducenti.Size = new System.Drawing.Size(760, 400);
            this.dataGridViewConducenti.TabIndex = 0;
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
            // ConducentiListForm
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
            this.Controls.Add(this.dataGridViewConducenti);
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.Name = "ConducentiListForm";
            this.Text = "Gestione Conducenti";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewConducenti)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.DataGridView dataGridViewConducenti;
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
