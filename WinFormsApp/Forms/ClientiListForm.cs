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

namespace FormulariRif_G.Forms
{
    public partial class ClientiListForm : Form
    {
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<Configurazione> _configurazioneRepository; 
        // Per risolvere i form di dettaglio
        private readonly IServiceProvider _serviceProvider;
        // Timer per il debouncing
        private System.Windows.Forms.Timer _searchTimer;
        // Per la cancellazione delle query
        private CancellationTokenSource _cancellationTokenSource; 


        public ClientiListForm(IGenericRepository<Cliente> clienteRepository,
                                 IGenericRepository<Configurazione> configurazioneRepository, // Aggiunta la dipendenza
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _clienteRepository = clienteRepository;
            _configurazioneRepository = configurazioneRepository;
            _serviceProvider = serviceProvider;
            this.Load += ClientiListForm_Load; // Carica i dati all'avvio del form

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
        }

        /// <summary>
        /// Carica i dati dei clienti nella DataGridView, filtrando per dati di test se necessario.
        /// </summary>
        private async Task LoadClientiAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // **Cancellare l'operazione precedente se presente**
                _cancellationTokenSource?.Cancel(); // Annulla qualsiasi operazione precedente
                _cancellationTokenSource?.Dispose(); // Rilascia le risorse del vecchio CancellationTokenSource
                _cancellationTokenSource = new CancellationTokenSource(); // Crea un nuovo CancellationTokenSource
                cancellationToken = _cancellationTokenSource.Token; // Assegna il token per questa operazione

                var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
                bool showTestData = configurazione?.DatiTest ?? false;

                IQueryable<Cliente> query = _clienteRepository.AsQueryable(); 
                // Includi Indirizzi e Contatti per poter accedere ai dati di navigazione
                query = query.Include(c => c.Indirizzi)
                             .Include(c => c.Contatti);
                if (!showTestData)
                    // Carica solo i clienti non di test                
                    query = query.Where(c => c.IsTestData == false);
                //Applica il filtro di ricerca se presente**
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
                // Nasconde le colonne di navigazione 
                if (dataGridViewClienti.Columns.Contains("Contatti"))                
                    dataGridViewClienti.Columns["Contatti"].Visible = false;                
                if (dataGridViewClienti.Columns.Contains("Indirizzi"))                
                    dataGridViewClienti.Columns["Indirizzi"].Visible = false;
                // Nasconde la colonna IsTestData (sebbene utile per il debug, non è per l'utente finale)
                if (dataGridViewClienti.Columns.Contains("IsTestData"))                
                    dataGridViewClienti.Columns["IsTestData"].Visible = false;                              
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei clienti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo".
        /// Apre il form di dettaglio per l'inserimento di un nuovo cliente.
        /// </summary>
        private async void btnNuovo_Click(object sender, EventArgs e)
        {
            using (var detailForm = _serviceProvider.GetRequiredService<ClientiDetailForm>())
            {
                // Imposta il form di dettaglio per l'inserimento di un nuovo record
                detailForm.SetCliente(new Cliente());
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadClientiAsync(); // Ricarica i dati dopo l'inserimento
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica".
        /// Apre il form di dettaglio per la modifica del cliente selezionato.
        /// </summary>
        private async void btnModifica_Click(object sender, EventArgs e)
        {
            if (dataGridViewClienti.SelectedRows.Count > 0)
            {
                var selectedId = (int)dataGridViewClienti.SelectedRows[0].Cells["Id"].Value;                
                var selectedCliente = await _clienteRepository.GetByIdAsync(selectedId);
                if (selectedCliente != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<ClientiDetailForm>())
                    {
                        detailForm.SetCliente(selectedCliente);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadClientiAsync(); // Ricarica i dati dopo la modifica
                        }
                    }
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
                //var selectedCliente = dataGridViewClienti.SelectedRows[0].DataBoundItem as Cliente;
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
        /// Apre il form di dettaglio in modalità sola lettura per il cliente selezionato.
        /// </summary>
        private async void btnDettagli_Click(object sender, EventArgs e)
        {
            if (dataGridViewClienti.SelectedRows.Count > 0)
            {
                // Recupera l'ID del cliente dalla riga selezionata (assumendo che Id sia una colonna nel DTO anonimo)
                // Usiamo Cells["Id"].Value perché DataBoundItem è ora un tipo anonimo.
                var selectedId = (int)dataGridViewClienti.SelectedRows[0].Cells["Id"].Value;

                // Carica il cliente completo dal repository, includendo le entità correlate
                var selectedCliente = await _clienteRepository.GetByIdAsync(selectedId);
                if (selectedCliente != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<ClientiDetailForm>())
                    {
                        detailForm.SetCliente(selectedCliente, isReadOnly: true);
                        detailForm.ShowDialog(); // Mostra il form in sola lettura
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un cliente per visualizzare i dettagli.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        /// Filtra i clienti visualizzati nella DataGridView.
        /// </summary>
        private async void txtRicerca_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop(); // Resetta il timer ad ogni battitura
            _searchTimer.Start(); // Riavvia il timer
            //try
            //{
            //    var searchText = txtRicerca.Text.Trim().ToLower(); // Converti a minuscolo per ricerca case-insensitive
            //    var configurazione = (await _configurazioneRepository.GetAllAsync()).FirstOrDefault();
            //    bool showTestData = configurazione?.DatiTest ?? false;

            //    IQueryable<Cliente> query = _clienteRepository.AsQueryable();
            //    // Includi Indirizzi e Contatti per poterli usare nel filtro di ricerca
            //    query = query.Include(c => c.Indirizzi)
            //                 .Include(c => c.Contatti);

            //    if (!showTestData)                
            //        query = query.Where(c => c.IsTestData == false);               
            //    if (!string.IsNullOrEmpty(searchText))
            //    {
            //        // Filtra per RagSoc, Indirizzo o Comune predefiniti, Telefono o Email predefiniti
            //        query = query.Where(c =>
            //            c.RagSoc.ToLower().Contains(searchText) ||
            //            c.Indirizzi.Any(ind => ind.Predefinito && (ind.Indirizzo.ToLower().Contains(searchText) || ind.Comune.ToLower().Contains(searchText))) ||
            //            c.Contatti.Any(cont => cont.Predefinito && (cont.Telefono.ToLower().Contains(searchText) || cont.Email.ToLower().Contains(searchText)))
            //        );
            //    }
            //    var filteredClienti = await query.ToListAsync();

            //    var clientiViewModel = filteredClienti.Select(c => new
            //    {
            //        c.Id,
            //        c.RagSoc,
            //        c.PartitaIva,
            //        c.CodiceFiscale,
            //        TelefonoPredefinito = c.Contatti.FirstOrDefault(cont => cont.Predefinito)?.Telefono ?? "N/D",
            //        EmailPredefinito = c.Contatti.FirstOrDefault(cont => cont.Predefinito)?.Email ?? "N/D",
            //        IndirizzoCompleto = c.Indirizzi.FirstOrDefault(ind => ind.Predefinito) != null ?
            //                            $"{c.Indirizzi.FirstOrDefault(ind => ind.Predefinito)?.Indirizzo}, " +
            //                            $"{c.Indirizzi.FirstOrDefault(ind => ind.Predefinito)?.Comune} - " +
            //                            $"{c.Indirizzi.FirstOrDefault(ind => ind.Predefinito)?.Cap} "
            //                            : "N/D",
            //        c.IsTestData // Manteniamo questa proprietà per la logica interna, ma la colonna sarà nascosta
            //    }).ToList();

            //    dataGridViewClienti.DataSource = clientiViewModel;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Errore durante la ricerca: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
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

        // Codice generato dal designer per ClientiListForm
        #region Windows Form Designer generated code

        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

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
