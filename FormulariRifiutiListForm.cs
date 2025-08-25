// File: Forms/FormulariRifiutiListForm.cs
// Questo form visualizza un elenco di formulari rifiuti e permette le operazioni CRUD.
// Ora utilizza FormManager per la gestione delle form di dettaglio non modali.
using Microsoft.Extensions.DependencyInjection;
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.EntityFrameworkCore; // Per l'include delle proprietà di navigazione
using System.Threading;
using System.Windows.Forms;
using FormulariRif_G.Service; // Aggiunto l'using per FormManager

namespace FormulariRif_G.Forms
{
    public partial class FormulariRifiutiListForm : Form
    {
        private readonly IGenericRepository<FormularioRifiuti> _formularioRifiutiRepository;
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<Automezzo> _automezzoRepository;
        private DateTimePicker dtpDaData;
        private Label label1;
        private readonly FormManager _formManager; // Sostituito IServiceProvider con FormManager

        private System.Windows.Forms.Timer _searchTimer;
        private CancellationTokenSource _cancellationTokenSource;

        public FormulariRifiutiListForm(IGenericRepository<FormularioRifiuti> formularioRifiutiRepository,
                                         IGenericRepository<Cliente> clienteRepository,
                                         IGenericRepository<Automezzo> automezzoRepository,
                                         FormManager formManager) // Inietta FormManager
        {
            InitializeComponent();
            _formularioRifiutiRepository = formularioRifiutiRepository;
            _clienteRepository = clienteRepository;
            _automezzoRepository = automezzoRepository;
            _formManager = formManager; // Assegna il FormManager iniettato
            this.Load += FormulariRifiutiListForm_Load;

            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 500;
            _searchTimer.Tick += SearchTimer_Tick;
            _searchTimer.Stop();
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
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = _cancellationTokenSource.Token;
 
                IQueryable<FormularioRifiuti> formulariQuery = _formularioRifiutiRepository.AsQueryable()
                    .Include(f => f.Produttore)
                    .Include(f => f.ProduttoreIndirizzo)
                    .Include(f => f.Automezzo);
 
                if (dtpDaData.Value != null)
                {
                    formulariQuery = formulariQuery.Where(f => f.Data >= dtpDaData.Value);
                }
 
                // La proiezione ora utilizza le proprietà di navigazione caricate da .Include().
                // Questo è molto più efficiente perché evita di caricare intere tabelle in memoria.
                var displayData = await formulariQuery.Select(f => new
                {
                    f.Id,
                    f.Data,
                    f.NumeroFormulario,
                    Cliente = f.Produttore != null ? f.Produttore.RagSoc : "Sconosciuto",
                    // Dopo aver aggiunto IndirizzoCompleto a ClienteIndirizzo, questo funzionerà
                    Indirizzo = f.ProduttoreIndirizzo != null ? f.ProduttoreIndirizzo.IndirizzoCompleto : "Sconosciuto",
                    Automezzo = f.Automezzo != null ? f.Automezzo.Descrizione : "Sconosciuto"
                }).ToListAsync(cancellationToken);
 
                dataGridViewFormulari.DataSource = displayData;
                dataGridViewFormulari.Columns["Id"].Visible = false;

                // Miglioramento: personalizza le intestazioni e il formato delle colonne
                dataGridViewFormulari.Columns["NumeroFormulario"].HeaderText = "Numero Formulario";
                dataGridViewFormulari.Columns["Data"].DefaultCellStyle.Format = "dd/MM/yyyy";
                dataGridViewFormulari.Columns["Cliente"].HeaderText = "Ragione Sociale";
                dataGridViewFormulari.Columns["Indirizzo"].Width = 250; // Imposta una larghezza fissa per l'indirizzo
                dataGridViewFormulari.Columns["Automezzo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Riempe lo spazio rimanente
            }
            catch (Exception ex)
            {
                // Gestisci l'eccezione OperationCanceledException se è dovuta alla cancellazione.
                if (ex is OperationCanceledException)
                {
                    // L'operazione è stata cancellata, non è un errore da mostrare all'utente.
                    // Puoi loggare o ignorare.
                }
                else
                {
                    MessageBox.Show($"Errore durante il caricamento dei formulari rifiuti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// Apre FormulariRifiutiDetailForm in modalità non modale per l'inserimento.
        /// </summary>
        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            // Usa il FormManager per aprire o attivare la FormulariRifiutiDetailForm
            var detailForm = _formManager.ShowOrActivate<FormulariRifiutiDetailForm>();

            // Imposta il formulario e la modalità.
            // Se la form era già aperta, potremmo volerla resettare o meno,
            // a seconda della logica desiderata. Qui la resettiamo a una nuova entità.
            detailForm.SetFormulario(new FormularioRifiuti { Data = DateTime.Now });

            // Per le form non modali, gestiamo l'evento FormClosed
            // per sapere quando ricaricare i dati.
            detailForm.FormClosed -= DetailForm_FormClosed; // Rimuovi per evitare duplicati
            detailForm.FormClosed += DetailForm_FormClosed; // Aggiungi il gestore
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// Apre FormulariRifiutiDetailForm in modalità non modale per la modifica.
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewFormulari.SelectedRows.Count > 0)
            {
                var selectedFormularioId = (int)dataGridViewFormulari.SelectedRows[0].Cells["Id"].Value;
                var selectedFormulario = await _formularioRifiutiRepository.GetByIdAsync(selectedFormularioId);

                if (selectedFormulario != null)
                {
                    // Usa il FormManager per aprire o attivare la FormulariRifiutiDetailForm
                    var detailForm = _formManager.ShowOrActivate<FormulariRifiutiDetailForm>();
                    detailForm.SetFormulario(selectedFormulario);

                    detailForm.FormClosed -= DetailForm_FormClosed; // Rimuovi per evitare duplicati
                    detailForm.FormClosed += DetailForm_FormClosed; // Aggiungi il gestore
                }
            }
            else
            {
                MessageBox.Show("Seleziona un formulario da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestore comune per l'evento FormClosed delle form di dettaglio.
        /// Ricarica i dati quando una form di dettaglio viene chiusa.
        /// </summary>
        private async void DetailForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            await LoadFormulariRifiutiAsync();
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
            if (disposing)
            {
                _searchTimer?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            base.Dispose(disposing);
        }

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
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            await LoadFormulariRifiutiAsync();
        }
    }
}
