// File: Forms/FormulariRifiutiListForm.cs
// Questo form visualizza un elenco di formulari rifiuti e permette le operazioni CRUD.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione
using System.Threading; // Aggiungi questo per CancellationTokenSource
using System.Windows.Forms; // Per Timer (se usi System.Windows.Forms.Timer)


namespace FormulariRif_G.Forms
{
    public partial class FormulariRifiutiListForm : Form
    {
        private readonly IGenericRepository<FormularioRifiuti> _formularioRifiutiRepository;
        private readonly IGenericRepository<Cliente> _clienteRepository; // Per caricare i nomi dei clienti
        private readonly IGenericRepository<Automezzo> _automezzoRepository; // Per caricare le descrizioni degli automezzi
        private DateTimePicker dtpDaData;
        private Label label1;
        private readonly IServiceProvider _serviceProvider;
        // Timer per il debouncing
        private System.Windows.Forms.Timer _searchTimer;
        // Per la cancellazione delle query
        private CancellationTokenSource _cancellationTokenSource;

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

            // **Inizializza il timer per il debouncing**
            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 500; // Aspetta 500ms dopo l'ultima battitura
            _searchTimer.Tick += SearchTimer_Tick; // Collega l'evento Tick al metodo di ricerca
            _searchTimer.Stop(); // Il timer inizia fermo
        }

        private async void FormulariRifiutiListForm_Load(object? sender, EventArgs e)
        {
            dtpDaData.Value = DateTime.Now.AddDays(-60);
            await LoadFormulariRifiutiAsync();
        }

        /// <summary>
        /// Carica i formulari rifiuti nella DataGridView, includendo i dati correlati.
        /// </summary>
        private async Task LoadFormulariRifiutiAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // **Cancellare l'operazione precedente se presente**
                _cancellationTokenSource?.Cancel(); // Annulla qualsiasi operazione precedente
                _cancellationTokenSource?.Dispose(); // Rilascia le risorse del vecchio CancellationTokenSource
                _cancellationTokenSource = new CancellationTokenSource(); // Crea un nuovo CancellationTokenSource
                cancellationToken = _cancellationTokenSource.Token; // Assegna il token per questa operazione                                

                // Carica i formulari includendo le proprietà di navigazione per Cliente, ClienteIndirizzo e Automezzo
                // Nota: GenericRepository.GetAllAsync() non include le proprietà di navigazione per default.
                // Dobbiamo estendere IGenericRepository o creare un metodo specifico per l'inclusione.
                // Per ora, useremo un approccio diretto per la query con include.
                // Se il tuo GenericRepository non supporta Include, questo potrebbe richiedere una modifica.
                // Assumendo che AppDbContext sia accessibile o che tu abbia un modo per fare Include.
                //var formulari = await _formularioRifiutiRepository.GetAllAsync();
                IQueryable<FormularioRifiuti> formulariQuery = _formularioRifiutiRepository.AsQueryable()
                    .Include(f => f.Cliente)
                    .Include(f => f.ClienteIndirizzo)
                    .Include(f => f.Automezzo);
                if (dtpDaData.Value != null)
                {
                    formulariQuery = formulariQuery.Where(f => f.Data >= dtpDaData.Value);
                }

                // Per visualizzare i nomi/descrizioni, dobbiamo unire i dati
                var clienti = (await _clienteRepository.GetAllAsync()).ToDictionary(c => c.Id, c => c.RagSoc);
                var automezzi = (await _automezzoRepository.GetAllAsync()).ToDictionary(a => a.Id, a => a.Descrizione);
                var indirizzi = (await _serviceProvider.GetRequiredService<IGenericRepository<ClienteIndirizzo>>().GetAllAsync())
                                .ToDictionary(ci => ci.Id, ci => $"{ci.Indirizzo}, {ci.Comune}");

                var displayData = formulariQuery.Select(f => new
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

        ///// <summary>
        ///// Clean up any resources being used.
        ///// </summary>
        ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridViewFormulari = new DataGridView();
            btnNuovo = new Button();
            btnModifica = new Button();
            btnElimina = new Button();
            btnAggiorna = new Button();
            dtpDaData = new DateTimePicker();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridViewFormulari).BeginInit();
            SuspendLayout();
            // 
            // dataGridViewFormulari
            // 
            dataGridViewFormulari.AllowUserToAddRows = false;
            dataGridViewFormulari.AllowUserToDeleteRows = false;
            dataGridViewFormulari.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewFormulari.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewFormulari.Location = new Point(12, 49);
            dataGridViewFormulari.MultiSelect = false;
            dataGridViewFormulari.Name = "dataGridViewFormulari";
            dataGridViewFormulari.ReadOnly = true;
            dataGridViewFormulari.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewFormulari.Size = new Size(760, 363);
            dataGridViewFormulari.TabIndex = 0;
            // 
            // btnNuovo
            // 
            btnNuovo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNuovo.Location = new Point(12, 420);
            btnNuovo.Name = "btnNuovo";
            btnNuovo.Size = new Size(75, 30);
            btnNuovo.TabIndex = 1;
            btnNuovo.Text = "Nuovo";
            btnNuovo.UseVisualStyleBackColor = true;
            btnNuovo.Click += btnNuovo_Click;
            // 
            // btnModifica
            // 
            btnModifica.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnModifica.Location = new Point(93, 420);
            btnModifica.Name = "btnModifica";
            btnModifica.Size = new Size(75, 30);
            btnModifica.TabIndex = 2;
            btnModifica.Text = "Modifica";
            btnModifica.UseVisualStyleBackColor = true;
            btnModifica.Click += btnModifica_Click;
            // 
            // btnElimina
            // 
            btnElimina.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnElimina.Location = new Point(174, 420);
            btnElimina.Name = "btnElimina";
            btnElimina.Size = new Size(75, 30);
            btnElimina.TabIndex = 3;
            btnElimina.Text = "Elimina";
            btnElimina.UseVisualStyleBackColor = true;
            btnElimina.Click += btnElimina_Click;
            // 
            // btnAggiorna
            // 
            btnAggiorna.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAggiorna.Location = new Point(697, 420);
            btnAggiorna.Name = "btnAggiorna";
            btnAggiorna.Size = new Size(75, 30);
            btnAggiorna.TabIndex = 4;
            btnAggiorna.Text = "Aggiorna";
            btnAggiorna.UseVisualStyleBackColor = true;
            btnAggiorna.Click += btnAggiorna_Click;
            // 
            // dtpDaData
            // 
            dtpDaData.Format = DateTimePickerFormat.Short;
            dtpDaData.Location = new Point(81, 12);
            dtpDaData.Name = "dtpDaData";
            dtpDaData.Size = new Size(131, 23);
            dtpDaData.TabIndex = 19;
            dtpDaData.ValueChanged += dtpDaData_ValueChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 17);
            label1.Name = "label1";
            label1.Size = new Size(47, 15);
            label1.TabIndex = 20;
            label1.Text = "Da data";
            // 
            // FormulariRifiutiListForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(label1);
            Controls.Add(dtpDaData);
            Controls.Add(btnAggiorna);
            Controls.Add(btnElimina);
            Controls.Add(btnModifica);
            Controls.Add(btnNuovo);
            Controls.Add(dataGridViewFormulari);
            MinimumSize = new Size(800, 500);
            Name = "FormulariRifiutiListForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Gestione Formulari Rifiuti";
            ((System.ComponentModel.ISupportInitialize)dataGridViewFormulari).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        private System.Windows.Forms.DataGridView dataGridViewFormulari;
        private System.Windows.Forms.Button btnNuovo;
        private System.Windows.Forms.Button btnModifica;
        private System.Windows.Forms.Button btnElimina;
        private System.Windows.Forms.Button btnAggiorna;

        #endregion

        private void dtpDaData_ValueChanged(object sender, EventArgs e)
        {
            // Resetta e riavvia il timer ad ogni battitura
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        /// <summary>
        /// Questo metodo viene chiamato quando il timer di ricerca scatta.
        /// Indica che l'utente ha smesso di digitare per un po', quindi eseguiamo la ricerca.
        /// </summary>
        private async void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop(); // Ferma il timer per evitare che scatti di nuovo immediatamente
            await LoadFormulariRifiutiAsync(); // Esegui la ricerca effettiva
        }

        // **Aggiungi Dispose per pulire il CancellationTokenSource e il Timer**
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                _searchTimer?.Dispose(); // Assicurati di disporre il timer
                _cancellationTokenSource?.Dispose(); // Assicurati di disporre il CancellationTokenSource
            }
            base.Dispose(disposing);
        }
    }
}
