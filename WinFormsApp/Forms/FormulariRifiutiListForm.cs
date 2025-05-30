// File: Forms/FormulariRifiutiListForm.cs
// Questo form visualizza un elenco di formulari rifiuti e permette le operazioni CRUD.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione

namespace FormulariRif_G.Forms
{
    public partial class FormulariRifiutiListForm : Form
    {
        private readonly IGenericRepository<FormularioRifiuti> _formularioRifiutiRepository;
        private readonly IGenericRepository<Cliente> _clienteRepository; // Per caricare i nomi dei clienti
        private readonly IGenericRepository<Automezzo> _automezzoRepository; // Per caricare le descrizioni degli automezzi
        private readonly IServiceProvider _serviceProvider;

        public FormulariRifiutiListForm(IGenericRepository<FormularioRifiuti> formularioRifiutiRepository,
                                        IGenericRepository<Cliente> clienteRepository,
                                        IGenericRepository<Automezzo> automezzoRepository,
                                        IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _formularioRifiutiRepository = formularioRifiutiRepository;
            _clienteRepository = clienteRepository;
            _automezzoRepository = automezzoRepository;
            _serviceProvider = serviceProvider;
            this.Load += FormulariRifiutiListForm_Load;
        }

        private async void FormulariRifiutiListForm_Load(object? sender, EventArgs e)
        {
            await LoadFormulariRifiutiAsync();
        }

        /// <summary>
        /// Carica i formulari rifiuti nella DataGridView, includendo i dati correlati.
        /// </summary>
        private async Task LoadFormulariRifiutiAsync()
        {
            try
            {
                // Carica i formulari includendo le proprietà di navigazione per Cliente, ClienteIndirizzo e Automezzo
                // Nota: GenericRepository.GetAllAsync() non include le proprietà di navigazione per default.
                // Dobbiamo estendere IGenericRepository o creare un metodo specifico per l'inclusione.
                // Per ora, useremo un approccio diretto per la query con include.
                // Se il tuo GenericRepository non supporta Include, questo potrebbe richiedere una modifica.
                // Assumendo che AppDbContext sia accessibile o che tu abbia un modo per fare Include.
                var formulari = await _formularioRifiutiRepository.GetAllAsync();

                // Per visualizzare i nomi/descrizioni, dobbiamo unire i dati
                var clienti = (await _clienteRepository.GetAllAsync()).ToDictionary(c => c.Id, c => c.RagSoc);
                var automezzi = (await _automezzoRepository.GetAllAsync()).ToDictionary(a => a.Id, a => a.Descrizione);
                var indirizzi = (await _serviceProvider.GetRequiredService<IGenericRepository<ClienteIndirizzo>>().GetAllAsync())
                                .ToDictionary(ci => ci.Id, ci => $"{ci.Indirizzo}, {ci.Comune}");

                var displayData = formulari.Select(f => new
                {
                    f.Id,
                    f.Data,
                    f.NumeroFormulario,
                    Cliente = clienti.GetValueOrDefault(f.IdCli, "Sconosciuto"),
                    Indirizzo = indirizzi.GetValueOrDefault(f.IdClienteIndirizzo, "Sconosciuto"),
                    Automezzo = automezzi.GetValueOrDefault(f.IdAutomezzo, "Sconosciuto")
                }).ToList();

                dataGridViewFormulari.DataSource = displayData;
                dataGridViewFormulari.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei formulari rifiuti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// </summary>
        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            using (var detailForm = _serviceProvider.GetRequiredService<FormulariRifiutiDetailForm>())
            {
                detailForm.SetFormulario(new FormularioRifiuti { Data = DateTime.Now });
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadFormulariRifiutiAsync();
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewFormulari.SelectedRows.Count > 0)
            {
                var selectedFormularioId = (int)dataGridViewFormulari.SelectedRows[0].Cells["Id"].Value;
                var selectedFormulario = await _formularioRifiutiRepository.GetByIdAsync(selectedFormularioId);

                if (selectedFormulario != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<FormulariRifiutiDetailForm>())
                    {
                        detailForm.SetFormulario(selectedFormulario);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadFormulariRifiutiAsync();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un formulario da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina".
        /// </summary>
        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewFormulari.SelectedRows.Count > 0)
            {
                var selectedFormularioId = (int)dataGridViewFormulari.SelectedRows[0].Cells["Id"].Value;
                var selectedFormulario = await _formularioRifiutiRepository.GetByIdAsync(selectedFormularioId);

                if (selectedFormulario != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il formulario numero '{selectedFormulario.NumeroFormulario}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _formularioRifiutiRepository.Delete(selectedFormulario);
                            await _formularioRifiutiRepository.SaveChangesAsync();
                            await LoadFormulariRifiutiAsync();
                            MessageBox.Show("Formulario eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione del formulario: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un formulario da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Aggiorna".
        /// </summary>
        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            await LoadFormulariRifiutiAsync();
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
            this.dataGridViewFormulari = new System.Windows.Forms.DataGridView();
            this.btnNuovo = new System.Windows.Forms.Button();
            this.btnModifica = new System.Windows.Forms.Button();
            this.btnElimina = new System.Windows.Forms.Button();
            this.btnAggiorna = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFormulari)).BeginInit();
            this.SuspendLayout();
            //
            // dataGridViewFormulari
            //
            this.dataGridViewFormulari.AllowUserToAddRows = false;
            this.dataGridViewFormulari.AllowUserToDeleteRows = false;
            this.dataGridViewFormulari.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewFormulari.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFormulari.Location = new System.Drawing.Point(12, 12);
            this.dataGridViewFormulari.MultiSelect = false;
            this.dataGridViewFormulari.Name = "dataGridViewFormulari";
            this.dataGridViewFormulari.ReadOnly = true;
            this.dataGridViewFormulari.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewFormulari.Size = new System.Drawing.Size(760, 400);
            this.dataGridViewFormulari.TabIndex = 0;
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
            // FormulariRifiutiListForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.btnAggiorna);
            this.Controls.Add(this.btnElimina);
            this.Controls.Add(this.btnModifica);
            this.Controls.Add(this.btnNuovo);
            this.Controls.Add(this.dataGridViewFormulari);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "FormulariRifiutiListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestione Formulari Rifiuti";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFormulari)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.DataGridView dataGridViewFormulari;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnAggiorna;

        #endregion
    }
}
