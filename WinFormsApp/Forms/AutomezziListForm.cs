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
using System.ComponentModel; // Necessario per BindingList

namespace FormulariRif_G.Forms
{
    public partial class AutomezziListForm : Form
    {
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private readonly IServiceProvider _serviceProvider;

        // Dichiarazioni dei controlli (corrispondenti al tuo Designer.cs)
        private System.Windows.Forms.DataGridView dataGridViewAutomezzi;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnAggiorna;

        public AutomezziListForm(IGenericRepository<Automezzo> automezzoRepository,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _automezzoRepository = automezzoRepository;
            _serviceProvider = serviceProvider;

            // Collega gli handler degli eventi ai pulsanti e al form Load.
            this.Load += AutomezziListForm_Load;
            if (btnNuovo != null) btnNuovo.Click += btnNuovo_Click;
            if (btnModifica != null) btnModifica.Click += btnModifica_Click;
            if (btnElimina != null) btnElimina.Click += btnElimina_Click;
            if (btnAggiorna != null) btnAggiorna.Click += btnAggiorna_Click;
        }

        private async void AutomezziListForm_Load(object? sender, EventArgs e)
        {
            await LoadAutomezziAsync();
        }

        /// <summary>
        /// Carica gli automezzi nella DataGridView.
        /// </summary>
        private async Task LoadAutomezziAsync()
        {
            try
            {
                var automezzi = await _automezzoRepository.GetAllAsync();
                // Assicurati che il DataSource sia impostato correttamente
                dataGridViewAutomezzi.DataSource = new BindingList<Automezzo>(automezzi.ToList());

                // Nasconde la colonna "Id" se esiste
                if (dataGridViewAutomezzi.Columns.Contains("Id"))
                {
                    dataGridViewAutomezzi.Columns["Id"].Visible = false;
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
                // Recupera l'automezzo selezionato dalla riga della DataGridView
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
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewAutomezzi = new System.Windows.Forms.DataGridView();
            this.btnNuovo = new System.Windows.Forms.Button();
            this.btnModifica = new System.Windows.Forms.Button();
            this.btnElimina = new System.Windows.Forms.Button();
            this.btnAggiorna = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAutomezzi)).BeginInit();
            this.SuspendLayout();
            //
            // dataGridViewAutomezzi
            //
            this.dataGridViewAutomezzi.AllowUserToAddRows = false;
            this.dataGridViewAutomezzi.AllowUserToDeleteRows = false;
            this.dataGridViewAutomezzi.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewAutomezzi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAutomezzi.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewAutomezzi.MultiSelect = false;
            this.dataGridViewAutomezzi.Name = "dataGridViewAutomezzi";
            this.dataGridViewAutomezzi.ReadOnly = true;
            this.dataGridViewAutomezzi.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewAutomezzi.Size = new System.Drawing.Size(760, 400);
            this.dataGridViewAutomezzi.TabIndex = 0;
            //
            // btnNuovo
            //
            this.btnNuovo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNuovo.Location = new System.Drawing.Point(12, 420);
            this.btnNuovo.Name = "btnNuovo";
            this.btnNuovo.Size = new System.Drawing.Size(75, 30);
            this.btnNuovo.TabIndex = 1;
            this.btnNuovo.Text = "Nuovo";
            this.btnNuovo.UseVisualStyleBackColor = true;
            //
            // btnModifica
            //
            this.btnModifica.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnModifica.Location = new System.Drawing.Point(93, 420);
            this.btnModifica.Name = "btnModifica";
            this.btnModifica.Size = new System.Drawing.Size(75, 30);
            this.btnModifica.TabIndex = 2;
            this.btnModifica.Text = "Modifica";
            this.btnModifica.UseVisualStyleBackColor = true;
            //
            // btnElimina
            //
            this.btnElimina.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnElimina.Location = new System.Drawing.Point(174, 420);
            this.btnElimina.Name = "btnElimina";
            this.btnElimina.Size = new System.Drawing.Size(75, 30);
            this.btnElimina.TabIndex = 3;
            this.btnElimina.Text = "Elimina";
            this.btnElimina.UseVisualStyleBackColor = true;
            //
            // btnAggiorna
            //
            this.btnAggiorna.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAggiorna.Location = new System.Drawing.Point(697, 420);
            this.btnAggiorna.Name = "btnAggiorna";
            this.btnAggiorna.Size = new System.Drawing.Size(75, 30);
            this.btnAggiorna.TabIndex = 4;
            this.btnAggiorna.Text = "Aggiorna";
            this.btnAggiorna.UseVisualStyleBackColor = true;
            //
            // AutomezziListForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.btnAggiorna);
            this.Controls.Add(this.btnElimina);
            this.Controls.Add(this.btnModifica);
            this.Controls.Add(this.btnNuovo);
            this.Controls.Add(this.dataGridViewAutomezzi);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "AutomezziListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestione Automezzi";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAutomezzi)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}