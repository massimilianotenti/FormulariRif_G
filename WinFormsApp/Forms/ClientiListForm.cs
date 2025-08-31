// File: Forms/ClientiListForm.cs
// Questo form visualizza un elenco di clienti, permette la ricerca e le operazioni CRUD.
// Ora considera il flag "DatiTest" dalla configurazione per visualizzare i dati di test.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Linq; // Per l'estensione Where
using Microsoft.EntityFrameworkCore; // Necessario per il metodo Include
using System.Threading; // Aggiungi questo per CancellationTokenSource
using System.Windows.Forms; // Per Timer (se usi System.Windows.Forms.Timer)
using FormulariRif_G.Service; // NUOVO: Namespace per il FormManager

namespace FormulariRif_G.Forms
{
    public partial class ClientiListForm : Form
    {
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<Configurazione> _configurazioneRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly FormManager _formManager; // NUOVO: Riferimento al FormManager

        // Timer per il debouncing
        private System.Windows.Forms.Timer _searchTimer;
        // Per la cancellazione delle query
        private CancellationTokenSource _cancellationTokenSource;

        public ClientiListForm(IGenericRepository<Cliente> clienteRepository,
                               IGenericRepository<Configurazione> configurazioneRepository,
                               IServiceProvider serviceProvider,
                               FormManager formManager) // NUOVO: Inietta il FormManager
        {
            InitializeComponent();
            _clienteRepository = clienteRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;
            _formManager = formManager; // Inizializza il FormManager
            this.Load += ClientiListForm_Load;
            this.FormClosed += ClientiListForm_FormClosed; // NUOVO: Gestisce la chiusura del form

            // **Inizializza il timer per il debouncing**
            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 500; // Aspetta 500ms dopo l'ultima battitura
            _searchTimer.Tick += SearchTimer_Tick; // Collega l'evento Tick al metodo di ricerca
            _searchTimer.Stop(); // Il timer inizia fermo
        }

        // Metodo chiamato al caricamento del form
        private async void ClientiListForm_Load(object? sender, EventArgs e)
        {
            await LoadClientiAsync();
            txtRicerca?.Focus();
        }

        /// <summary>
        /// Gestisce la chiusura del form.
        /// </summary>
        private void ClientiListForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // Cancella e disattiva il CancellationTokenSource quando il form viene chiuso
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null; // Imposta a null per evitare usi successivi di un oggetto disposto
        }

        /// <summary>
        /// Carica i dati dei clienti nella DataGridView, filtrando per dati di test se necessario.
        /// </summary>
        private async Task LoadClientiAsync() // Rimosso CancellationToken dal parametro per gestirlo internamente
        {
            // Inizializza un nuovo CancellationTokenSource per questa operazione
            _cancellationTokenSource?.Cancel(); // Annulla qualsiasi operazione precedente in corso
            _cancellationTokenSource?.Dispose(); // Rilascia le risorse del vecchio CancellationTokenSource
            _cancellationTokenSource = new CancellationTokenSource(); // Crea un nuovo CancellationTokenSource
            var cancellationToken = _cancellationTokenSource.Token; // Ottieni il token per questa operazione

            try
            {
                // Mostra un indicatore di caricamento (es. una ProgressBar o un Label)
                // lblLoading.Visible = true; // Assumi di avere un lblLoading nel designer
                // panelLoading.Visible = true; // Assumi di avere un panelLoading nel designer

                var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
                bool showTestData = configurazione?.DatiTest ?? false;

                IQueryable<Cliente> query = _clienteRepository.AsQueryable();
                // Includi Indirizzi e Contatti per poter accedere ai dati di navigazione
                query = query.Include(c => c.Indirizzi)
                             .Include(c => c.Contatti);

                if (!showTestData)
                {
                    // Carica solo i clienti non di test
                    query = query.Where(c => c.IsTestData == false);
                }

                // Applica il filtro di ricerca se presente
                var searchText = txtRicerca.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(searchText))
                {
                    query = query.Where(c =>
                        c.RagSoc.ToLower().Contains(searchText) ||
                        c.CodiceFiscale.ToLower().Contains(searchText) ||
                        c.PartitaIva.ToLower().Contains(searchText) ||
                        c.Indirizzi.Any(ind => ind.Predefinito && (ind.Indirizzo.ToLower().Contains(searchText) || ind.Comune.ToLower().Contains(searchText))) ||
                        c.Contatti.Any(cont => cont.Predefinito && (cont.Telefono.ToLower().Contains(searchText) || cont.Email.ToLower().Contains(searchText)))
                    );
                }

                // Esegui la query per ottenere i clienti con i dati inclusi
                var clienti = await query.ToListAsync(cancellationToken);

                // Prepara i dati per la DataGridView, includendo indirizzo e contatto predefiniti
                var clientiViewModel = clienti.Select(c => new
                {
                    c.Id,
                    c.RagSoc,
                    c.PartitaIva,
                    c.CodiceFiscale,
                    TelefonoPredefinito = c.Contatti.FirstOrDefault(cont => cont.Predefinito)?.Telefono ?? "N/D",
                    EmailPredefinito = c.Contatti.FirstOrDefault(cont => cont.Predefinito)?.Email ?? "N/D",
                    IndirizzoCompleto = c.Indirizzi.FirstOrDefault(ind => ind.Predefinito) != null ?
                                        $"{c.Indirizzi.FirstOrDefault(ind => ind.Predefinito)?.Indirizzo}, " +
                                        $"{c.Indirizzi.FirstOrDefault(ind => ind.Predefinito)?.Comune} - " +
                                        $"{c.Indirizzi.FirstOrDefault(ind => ind.Predefinito)?.Cap} "
                                        : "N/D",
                    c.IsTestData // Manteniamo questa proprietà per la logica interna, ma la colonna sarà nascosta
                }).ToList();

                dataGridViewClienti.DataSource = clientiViewModel.ToList();
                // Nasconde la colonna Id per una migliore visualizzazione, ma la mantiene accessibile per le operazioni
                dataGridViewClienti.Columns["Id"].Visible = false;
                // Nasconde le colonne di navigazione (potrebbero non esistere nel DTO anonimo, ma è una buona precauzione)
                if (dataGridViewClienti.Columns.Contains("Contatti"))
                    dataGridViewClienti.Columns["Contatti"].Visible = false;
                if (dataGridViewClienti.Columns.Contains("Indirizzi"))
                    dataGridViewClienti.Columns["Indirizzi"].Visible = false;
                // Nasconde la colonna IsTestData (sebbene utile per il debug, non è per l'utente finale)
                if (dataGridViewClienti.Columns.Contains("IsTestData"))
                    dataGridViewClienti.Columns["IsTestData"].Visible = false;
            }
            catch (OperationCanceledException)
            {
                // L'operazione è stata cancellata, non fare nulla (silenziosamente)
                Console.WriteLine("ClientiListForm: Operazione di caricamento clienti annullata.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei clienti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Nascondi l'indicatore di caricamento
                // lblLoading.Visible = false;
                // panelLoading.Visible = false;
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// Apre il form di dettaglio per l'inserimento di un nuovo cliente (non modale).
        /// </summary>
        private void btnNuovo_Click(object sender, EventArgs e)
        {
            // Ottiene o crea la ClientiDetailForm tramite FormManager
            var detailForm = _formManager.ShowOrActivate<ClientiDetailForm>();
            detailForm.SetCliente(new Cliente()); // Imposta il form per un nuovo cliente
            // Iscriviti all'evento FormClosed per ricaricare i dati quando il dettaglio si chiude
            detailForm.FormClosed -= ClienteDetailForm_Closed; // Evita duplicati
            detailForm.FormClosed += ClienteDetailForm_Closed;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// Apre il form di dettaglio per la modifica del cliente selezionato (non modale).
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewClienti.SelectedRows.Count > 0)
            {
                var selectedId = (int)dataGridViewClienti.SelectedRows[0].Cells["Id"].Value;
                var selectedCliente = await _clienteRepository.GetByIdAsync(selectedId);
                if (selectedCliente != null)
                {
                    var detailForm = _formManager.ShowOrActivate<ClientiDetailForm>();
                    detailForm.SetCliente(selectedCliente); // Imposta il form per la modifica
                    // Iscriviti all'evento FormClosed per ricaricare i dati quando il dettaglio si chiude
                    detailForm.FormClosed -= ClienteDetailForm_Closed; // Evita duplicati
                    detailForm.FormClosed += ClienteDetailForm_Closed;
                }
            }
            else
            {
                MessageBox.Show("Seleziona un cliente da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina".
        /// Elimina il cliente selezionato dopo una richiesta di conferma.
        /// </summary>
        private async void btnElimina_Click(object sender, EventArgs e)
        {
            if (dataGridViewClienti.SelectedRows.Count > 0)
            {
                var selectedId = (int)dataGridViewClienti.SelectedRows[0].Cells["Id"].Value;
                var selectedCliente = await _clienteRepository.GetByIdAsync(selectedId);
                if (selectedCliente != null)
                {
                    var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il cliente '{selectedCliente.RagSoc}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        try
                        {
                            _clienteRepository.Delete(selectedCliente);
                            await _clienteRepository.SaveChangesAsync();
                            await LoadClientiAsync(); // Ricarica i dati dopo l'eliminazione
                            MessageBox.Show("Cliente eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione del cliente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un cliente da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Dettagli".
        /// Apre il form di dettaglio in modalità sola lettura per il cliente selezionato (non modale).
        /// </summary>
        private async void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewClienti.SelectedRows.Count > 0)
            {
                var selectedId = (int)dataGridViewClienti.SelectedRows[0].Cells["Id"].Value;

                var selectedCliente = await _clienteRepository.GetByIdAsync(selectedId);
                if (selectedCliente != null)
                {
                    var detailForm = _formManager.ShowOrActivate<ClientiDetailForm>();
                    detailForm.SetCliente(selectedCliente, isReadOnly: true); // Imposta il form in sola lettura
                    // Non è necessario iscriversi a FormClosed qui perché la modalità solo lettura non implica modifiche da ricaricare
                }
            }
            else
            {
                MessageBox.Show("Seleziona un cliente per visualizzare i dettagli.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Metodo per gestire l'evento FormClosed della ClientiDetailForm.
        /// Ricarica i dati dei clienti nella lista principale.
        /// </summary>
        private async void ClienteDetailForm_Closed(object? sender, FormClosedEventArgs e)
        {
            // Ricarica i dati nella lista principale quando il form di dettaglio si chiude
            await LoadClientiAsync();

            // Disiscrivi l'evento per evitare riferimenti persistenti
            if (sender is ClientiDetailForm detailForm)
            {
                detailForm.FormClosed -= ClienteDetailForm_Closed;
            }
        }


        /// <summary>
        /// Gestisce il click sul pulsante "Aggiorna".
        /// Ricarica i dati dei clienti.
        /// </summary>
        private async void btnAggiorna_Click(object sender, EventArgs e)
        {
            await LoadClientiAsync();
            txtRicerca.Clear(); // Pulisce il campo di ricerca
        }

        /// <summary>
        /// Gestisce il cambio di testo nel campo di ricerca.
        /// Attiva il timer per il debouncing.
        /// </summary>
        private void txtRicerca_TextChanged(object sender, EventArgs e)
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
            await LoadClientiAsync(); // Esegui la ricerca effettiva
        }

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
                _searchTimer?.Dispose(); // Assicurati di disporre il timer
                // Il CancellationTokenSource viene disposto nell'evento FormClosed per evitare race conditions
                // durante le operazioni asincrone in corso al momento della chiusura del form.
                // Se non c'è un'operazione in corso, può essere nullo.
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewClienti = new System.Windows.Forms.DataGridView();
            this.btnNuovo = new System.Windows.Forms.Button();
            this.btnModifica = new System.Windows.Forms.Button();
            this.btnElimina = new System.Windows.Forms.Button();
            this.btnDettagli = new System.Windows.Forms.Button();
            this.btnAggiorna = new System.Windows.Forms.Button();
            this.txtRicerca = new System.Windows.Forms.TextBox();
            this.lblRicerca = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClienti)).BeginInit();
            this.SuspendLayout();
            //
            // dataGridViewClienti
            //
            this.dataGridViewClienti.AllowUserToAddRows = false;
            this.dataGridViewClienti.AllowUserToDeleteRows = false;
            this.dataGridViewClienti.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewClienti.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewClienti.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewClienti.Location = new System.Drawing.Point(12, 60);
            this.dataGridViewClienti.MultiSelect = false;
            this.dataGridViewClienti.Name = "dataGridViewClienti";
            this.dataGridViewClienti.ReadOnly = true;
            this.dataGridViewClienti.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewClienti.Size = new System.Drawing.Size(760, 400);
            this.dataGridViewClienti.TabIndex = 0;
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
            // ClientiListForm
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
            this.Controls.Add(this.dataGridViewClienti);
            this.MinimumSize = new System.Drawing.Size(800, 550);
            this.Name = "ClientiListForm";
            this.Text = "Gestione Clienti";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClienti)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.DataGridView dataGridViewClienti;
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