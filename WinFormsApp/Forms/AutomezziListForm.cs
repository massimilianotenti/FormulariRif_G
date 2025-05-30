// File: Forms/AutomezziListForm.cs
// Questo form visualizza un elenco di automezzi e permette le operazioni CRUD.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;

namespace FormulariRif_G.Forms
{
    public partial class AutomezziListForm : Form
    {
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private readonly IServiceProvider _serviceProvider;

        public AutomezziListForm(IGenericRepository<Automezzo> automezzoRepository,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _automezzoRepository = automezzoRepository;
            _serviceProvider = serviceProvider;
            this.Load += AutomezziListForm_Load;
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
                dataGridViewAutomezzi.DataSource = automezzi.ToList();
                dataGridViewAutomezzi.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli automezzi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// </summary>
        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            using (var detailForm = _serviceProvider.GetRequiredService<AutomezziDetailForm>())
            {
                detailForm.SetAutomezzo(new Automezzo());
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadAutomezziAsync();
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewAutomezzi.SelectedRows.Count > 0)
            {
                var selectedAutomezzo = dataGridViewAutomezzi.SelectedRows[0].DataBoundItem as Automezzo;
                if (selectedAutomezzo != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<AutomezziDetailForm>())
                    {
                        detailForm.SetAutomezzo(selectedAutomezzo);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadAutomezziAsync();
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
        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewAutomezzi.SelectedRows.Count > 0)
            {
                var selectedAutomezzo = dataGridViewAutomezzi.SelectedRows[0].DataBoundItem as Automezzo;
                if (selectedAutomezzo != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare l'automezzo '{selectedAutomezzo.Descrizione}' con targa '{selectedAutomezzo.Targa}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _automezzoRepository.Delete(selectedAutomezzo);
                            await _automezzoRepository.SaveChangesAsync();
                            await LoadAutomezziAsync();
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
        private async void btnAggiorna_Click(object sender, EventArgs e)
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
            this.btnNuovo.Click += new System.EventHandler(this.btnNuovo_Click);
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
            this.btnModifica.Click += new System.EventHandler(this.btnModifica_Click);
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
            this.btnElimina.Click += new System.EventHandler(this.btnElimina_Click);
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
            this.btnAggiorna.Click += new System.EventHandler(this.btnAggiorna_Click);
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

        private System.Windows.Forms.DataGridView dataGridViewAutomezzi;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnAggiorna;

        #endregion
    }
}
