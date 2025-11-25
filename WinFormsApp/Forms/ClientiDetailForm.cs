// File: Forms/ClientiDetailForm.cs
// Questo form permette di visualizzare, creare e modificare i dettagli di un cliente.
// Ora funziona in modalità master-detail, mostrando e permettendo la gestione
// degli indirizzi e dei contatti del cliente direttamente da qui.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.Extensions.DependencyInjection; // Per IServiceProvider
using System.Linq;
using Microsoft.EntityFrameworkCore; // Non strettamente necessario qui se usi solo FindAsync/GetByIdAsync, ma utile per contesto
using System.Windows.Forms; // Assicurati che sia presente per Form, MessageBox, DialogResult
using System; // Per EventArgs e Exception
using System.Threading.Tasks; // Per Task
using FormulariRif_G.Controls; // Per SearchableComboBox

namespace FormulariRif_G.Forms
{
    public partial class ClientiDetailForm : Form
    {
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<ClienteIndirizzo> _clienteIndirizzoRepository;
        private readonly IGenericRepository<ClienteContatto> _clienteContattoRepository;
        private readonly IGenericRepository<Tipo> _tipoRepository;
        private readonly IServiceProvider _serviceProvider;
        private Cliente? _currentCliente;
        private bool _isReadOnly;

        // Dichiarazioni dei controlli (corrispondenti al tuo Designer.cs)
        private System.Windows.Forms.Label lblRagSoc;
        private System.Windows.Forms.TextBox txtRagSoc;
        private System.Windows.Forms.Button btnSalva;
        private System.Windows.Forms.Label lblCodiceFiscale;
        private System.Windows.Forms.TextBox txtCodiceFiscale;
        private System.Windows.Forms.GroupBox groupBoxIndirizzi;
        private System.Windows.Forms.Button btnEliminaIndirizzo;
        private System.Windows.Forms.Button btnModificaIndirizzo;
        private System.Windows.Forms.Button btnNuovoIndirizzo;
        private System.Windows.Forms.DataGridView dataGridViewIndirizzi;
        private System.Windows.Forms.GroupBox groupBoxContatti;
        private System.Windows.Forms.Button btnEliminaContatto;
        private System.Windows.Forms.Button btnModificaContatto;
        private System.Windows.Forms.Button btnNuovoContatto;
        private System.Windows.Forms.DataGridView dataGridViewContatti;
        private System.Windows.Forms.TextBox txtPIVA;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIscrizAlbo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAutoCom;
        private System.Windows.Forms.Label label3;
        private SearchableComboBox cmbTipo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnAnnulla; // Aggiunto, se presente nel tuo designer

        public ClientiDetailForm(IGenericRepository<Cliente> clienteRepository,
                                 IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository,
                                 IGenericRepository<ClienteContatto> clienteContattoRepository,
                                 IGenericRepository<Tipo> tipoRepository,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _clienteRepository = clienteRepository;
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            _clienteContattoRepository = clienteContattoRepository;
            _tipoRepository = tipoRepository;
            _serviceProvider = serviceProvider;
            this.Load += ClientiDetailForm_Load;

            // Collega gli handler degli eventi dei pulsanti per indirizzi e contatti
            // Assicurati che questi pulsanti siano stati trascinati sul form nel designer
            // e che i loro nomi (Name property) corrispondano a quelli qui sotto.
            // Ho aggiunto controlli null per maggiore robustezza nel caso un pulsante non ci fosse.
            if (btnNuovoIndirizzo != null) btnNuovoIndirizzo.Click += btnNuovoIndirizzo_Click;
            if (btnModificaIndirizzo != null) btnModificaIndirizzo.Click += btnModificaIndirizzo_Click;
            if (btnEliminaIndirizzo != null) btnEliminaIndirizzo.Click += btnEliminaIndirizzo_Click;

            if (btnNuovoContatto != null) btnNuovoContatto.Click += btnNuovoContatto_Click;
            if (btnModificaContatto != null) btnModificaContatto.Click += btnModificaContatto_Click;
            if (btnEliminaContatto != null) btnEliminaContatto.Click += btnEliminaContatto_Click;

            if (btnSalva != null) btnSalva.Click += btnSalva_Click;
            if (btnAnnulla != null)
            {
                btnAnnulla.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            }
        }

        /// <summary>
        /// Carica i dati iniziali quando il form viene caricato.
        /// </summary>
        private async void ClientiDetailForm_Load(object? sender, EventArgs e)
        {
            await LoadTipiAsync();
            // await LoadDataAsync();            
        }

        private async Task LoadTipiAsync()
        {
            try
            {
                var tipi = await _tipoRepository.GetAllAsync();
                cmbTipo.DisplayMember = "Descrizione";
                cmbTipo.ValueMember = "Id";
                cmbTipo.DataSource = tipi.OrderBy(t => t.Descrizione).Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei tipi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Imposta il cliente da visualizzare o modificare e lo stato del form.
        /// </summary>
        /// <param name="cliente">L'oggetto Cliente.</param>
        /// <param name="isReadOnly">True per la modalità di sola lettura, False per la modifica.</param>
        public async void SetCliente(Cliente cliente, bool isReadOnly = false)
        {
            _currentCliente = cliente;
            _isReadOnly = isReadOnly;

            LoadClienteData();
            await LoadDataAsync();
            SetFormState();
        }

        /// <summary>
        /// Carica i dati del cliente nei controlli del form.
        /// </summary>
        private void LoadClienteData()
        {
            if (_currentCliente != null)
            {
                txtRagSoc.Text = _currentCliente.RagSoc;
                txtPIVA.Text = _currentCliente.PartitaIva;
                txtCodiceFiscale.Text = _currentCliente.CodiceFiscale;
                txtIscrizAlbo.Text = _currentCliente.Iscrizione_Albo ?? string.Empty;
                txtAutoCom.Text = _currentCliente.Auto_Comunicazione ?? string.Empty;

                if (_currentCliente.TipoId.HasValue)
                {
                    cmbTipo.SelectedValue = _currentCliente.TipoId.Value;
                }
                else
                {
                    cmbTipo.SelectedValue = 0;
                }
            }
            else
            {
                // Inizializza i campi per un nuovo cliente
                txtRagSoc.Text = string.Empty;
                txtPIVA.Text = string.Empty;
                txtCodiceFiscale.Text = string.Empty;
                txtIscrizAlbo.Text = string.Empty;
                txtAutoCom.Text = string.Empty;
                cmbTipo.SelectedValue = 0;
            }
        }

        /// <summary>
        /// Imposta lo stato dei controlli del form (sola lettura o modifica).
        /// </summary>
        private void SetFormState()
        {
            txtRagSoc.ReadOnly = _isReadOnly;
            txtPIVA.ReadOnly = _isReadOnly;
            txtCodiceFiscale.ReadOnly = _isReadOnly;
            txtIscrizAlbo.ReadOnly = _isReadOnly;
            txtAutoCom.ReadOnly = _isReadOnly;
            cmbTipo.Enabled = !_isReadOnly;
            if (btnSalva != null) btnSalva.Visible = !_isReadOnly;

            // I pulsanti di gestione indirizzi/contatti sono abilitati solo se il cliente è esistente e non in sola lettura
            bool enableSubCrud = !_isReadOnly && _currentCliente?.Id != 0;
            if (btnNuovoIndirizzo != null) btnNuovoIndirizzo.Enabled = enableSubCrud;
            if (btnModificaIndirizzo != null) btnModificaIndirizzo.Enabled = enableSubCrud;
            if (btnEliminaIndirizzo != null) btnEliminaIndirizzo.Enabled = enableSubCrud;
            if (btnNuovoContatto != null) btnNuovoContatto.Enabled = enableSubCrud;
            if (btnModificaContatto != null) btnModificaContatto.Enabled = enableSubCrud;
            if (btnEliminaContatto != null) btnEliminaContatto.Enabled = enableSubCrud;
        }

        /// <summary>
        /// Carica tutti i dati relativi al cliente (indirizzi e contatti).
        /// Chiamato al Load del form o dopo modifiche/aggiunte/eliminazioni.
        /// </summary>
        private async Task LoadDataAsync()
        {
            // Se è un cliente esistente
            if (_currentCliente?.Id != 0)
            {
                await LoadIndirizziAsync();
                await LoadContattiAsync();
            }
            // Per un nuovo cliente, le griglie rimarranno vuote fino al salvataggio
            // e i pulsanti di gestione indirizzi/contatti saranno disabilitati (gestito da SetFormState).
            // Riapplica lo stato per gestire l'abilitazione dei pulsanti secondari
            SetFormState();
        }

        /// <summary>
        /// Carica gli indirizzi del cliente nella DataGridView degli indirizzi.
        /// </summary>
        private async Task LoadIndirizziAsync()
        {
            if (_currentCliente == null || _currentCliente.Id == 0) return;

            try
            {
                var indirizzi = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentCliente.Id);
                // Proietta i dati in un tipo anonimo per evitare problemi di binding con proprietà complesse
                var displayIndirizzi = indirizzi.Select(i => new
                {
                    i.Id,
                    IndirizzoCompleto = $"{i.Indirizzo}, {i.Cap} {i.Comune} ", // Concatenazione per visualizzazione                    
                    i.Predefinito
                }).ToList();

                if (dataGridViewIndirizzi != null)
                {
                    dataGridViewIndirizzi.DataSource = displayIndirizzi;
                    // Nascondi la colonna Id se non necessaria per la visualizzazione
                    if (dataGridViewIndirizzi.Columns.Contains("Id"))
                        dataGridViewIndirizzi.Columns["Id"].Visible = false;
                    // Adatta le colonne alla dimensione del contenuto
                    dataGridViewIndirizzi.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento degli indirizzi: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Carica i contatti del cliente nella DataGridView dei contatti.
        /// </summary>
        private async Task LoadContattiAsync()
        {
            if (_currentCliente == null || _currentCliente.Id == 0) return;

            try
            {
                var contatti = await _clienteContattoRepository.FindAsync(cc => cc.IdCli == _currentCliente.Id);
                // Proietta i dati in un tipo anonimo per evitare problemi di binding con proprietà complesse
                var displayContatti = contatti.Select(c => new
                {
                    c.Id,
                    c.Contatto,
                    c.Telefono,
                    c.Email,
                    c.Predefinito
                }).ToList();

                if (dataGridViewContatti != null)
                {
                    dataGridViewContatti.DataSource = displayContatti;
                    // Nascondi la colonna Id se non necessaria per la visualizzazione
                    if (dataGridViewContatti.Columns.Contains("Id"))
                        dataGridViewContatti.Columns["Id"].Visible = false;
                    // Adatta le colonne alla dimensione del contenuto
                    dataGridViewContatti.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei contatti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
        private async void btnSalva_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            if (_currentCliente == null)
                _currentCliente = new Cliente();

            _currentCliente.RagSoc = txtRagSoc.Text.Trim();
            _currentCliente.PartitaIva = txtPIVA.Text.Trim();
            _currentCliente.CodiceFiscale = txtCodiceFiscale.Text.Trim();
            _currentCliente.Iscrizione_Albo = txtIscrizAlbo.Text.Trim();
            _currentCliente.Auto_Comunicazione = txtAutoCom.Text.Trim();

            if (cmbTipo.SelectedValue != null && (int)cmbTipo.SelectedValue > 0)
            {
                _currentCliente.TipoId = (int)cmbTipo.SelectedValue;
            }
            else
            {
                _currentCliente.TipoId = null;
            }

            try
            {
                if (_currentCliente.Id == 0) // Nuovo cliente
                {
                    await _clienteRepository.AddAsync(_currentCliente);
                    // Salva per ottenere l'ID del nuovo cliente
                    await _clienteRepository.SaveChangesAsync();
                    MessageBox.Show("Cliente salvato con successo! Ora puoi aggiungere indirizzi e contatti.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Segnala che il cliente è stato salvato
                    this.DialogResult = DialogResult.OK;
                    // Non chiudere il form per permettere l'aggiunta di indirizzi e contatti subito
                    // Ricarica lo stato per abilitare i pulsanti di gestione
                    SetFormState();
                }
                else
                {
                    // Cliente esistente
                    _clienteRepository.Update(_currentCliente);
                    await _clienteRepository.SaveChangesAsync();
                    MessageBox.Show("Cliente aggiornato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio del cliente: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Esegue la validazione dei campi di input del cliente.
        /// </summary>
        /// <returns>True se l'input è valido, altrimenti False.</returns>
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtRagSoc.Text))
            {
                MessageBox.Show("Ragione Sociale è un campo obbligatorio.", "Validazione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRagSoc.Focus();
                return false;
            }
            // Puoi aggiungere altre regole di validazione qui (es. Partita IVA, Codice Fiscale)
            return true;
        }

        /// <summary>
        /// Metodo helper per ottenere l'ID della riga selezionata in una DataGridView.
        /// </summary>
        /// <param name="dataGridView">La DataGridView da cui ottenere l'ID.</param>
        /// <returns>L'ID della riga selezionata, o 0 se nessuna riga è selezionata o l'ID non è valido.</returns>
        private int GetSelectedRowId(DataGridView dataGridView)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                // Usiamo Dynamic per accedere alla proprietà Id del tipo anonimo in modo sicuro.
                // In alternativa, si potrebbe usare reflection come GetProperty("Id").GetValue(selectedRowData)
                // ma Dynamic è più conciso per questo caso.
                dynamic selectedRowData = dataGridView.SelectedRows[0].DataBoundItem;
                return selectedRowData.Id;
            }
            return 0;
        }

        #region Gestione Indirizzi

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo Indirizzo".
        /// </summary>
        private async void btnNuovoIndirizzo_Click(object? sender, EventArgs e)
        {
            if (_currentCliente == null || _currentCliente.Id == 0)
            {
                MessageBox.Show("Salva il cliente prima di aggiungere indirizzi.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var detailForm = _serviceProvider.GetRequiredService<ClientiIndirizzoDetailForm>())
            {
                var newIndirizzo = new ClienteIndirizzo { IdCli = _currentCliente.Id };
                detailForm.SetIndirizzo(newIndirizzo);
                if (detailForm.ShowDialog() == DialogResult.OK)
                    // La logica di salvataggio e gestione del predefinito è nel detail form
                    // Ricarica la griglia dopo il salvataggio
                    await LoadIndirizziAsync();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica Indirizzo".
        /// </summary>
        private async void btnModificaIndirizzo_Click(object? sender, EventArgs e)
        {
            int selectedId = GetSelectedRowId(dataGridViewIndirizzi);
            if (selectedId == 0)
            {
                MessageBox.Show("Seleziona un indirizzo da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedIndirizzo = await _clienteIndirizzoRepository.GetByIdAsync(selectedId);
            if (selectedIndirizzo == null)
            {
                MessageBox.Show("Indirizzo non trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var detailForm = _serviceProvider.GetRequiredService<ClientiIndirizzoDetailForm>())
            {
                detailForm.SetIndirizzo(selectedIndirizzo);
                if (detailForm.ShowDialog() == DialogResult.OK)
                    await LoadIndirizziAsync();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina Indirizzo".
        /// </summary>
        private async void btnEliminaIndirizzo_Click(object? sender, EventArgs e)
        {
            int selectedId = GetSelectedRowId(dataGridViewIndirizzi);
            if (selectedId == 0)
            {
                MessageBox.Show("Seleziona un indirizzo da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedIndirizzo = await _clienteIndirizzoRepository.GetByIdAsync(selectedId);
            if (selectedIndirizzo == null)
            {
                MessageBox.Show("Indirizzo non trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Logica per prevenire l'eliminazione dell'unico indirizzo predefinito
            // Questo controllo è fondamentale per garantire che ci sia sempre un indirizzo predefinito.
            var allAddresses = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentCliente!.Id);
            if (allAddresses.Count() == 1 && selectedIndirizzo.Predefinito)
            {
                MessageBox.Show("Impossibile eliminare l'unico indirizzo predefinito. Un cliente deve avere almeno un indirizzo predefinito.", "Errore Eliminazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare l'indirizzo '{selectedIndirizzo.Indirizzo}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    _clienteIndirizzoRepository.Delete(selectedIndirizzo);
                    await _clienteIndirizzoRepository.SaveChangesAsync();
                    await LoadIndirizziAsync();

                    // Se l'indirizzo eliminato era predefinito e ce ne sono altri, imposta il primo come predefinito
                    var remainingAddresses = await _clienteIndirizzoRepository.FindAsync(ci => ci.IdCli == _currentCliente!.Id);
                    if (!remainingAddresses.Any(a => a.Predefinito) && remainingAddresses.Any())
                    {
                        var firstRemaining = remainingAddresses.First();
                        firstRemaining.Predefinito = true;
                        _clienteIndirizzoRepository.Update(firstRemaining);
                        await _clienteIndirizzoRepository.SaveChangesAsync();
                        await LoadIndirizziAsync();
                    }

                    MessageBox.Show("Indirizzo eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante l'eliminazione dell'indirizzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion        

        #region Gestione Contatti

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo Contatto".
        /// </summary>
        private async void btnNuovoContatto_Click(object? sender, EventArgs e)
        {
            if (_currentCliente == null || _currentCliente.Id == 0)
            {
                MessageBox.Show("Salva il cliente prima di aggiungere contatti.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var detailForm = _serviceProvider.GetRequiredService<ClientiContattiDetailForm>())
            {
                var newContatto = new ClienteContatto { IdCli = _currentCliente.Id };
                detailForm.SetContatto(newContatto);
                if (detailForm.ShowDialog() == DialogResult.OK)
                    await LoadContattiAsync();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica Contatto".
        /// </summary>
        private async void btnModificaContatto_Click(object? sender, EventArgs e)
        {
            int selectedId = GetSelectedRowId(dataGridViewContatti);
            if (selectedId == 0)
            {
                MessageBox.Show("Seleziona un contatto da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedContatto = await _clienteContattoRepository.GetByIdAsync(selectedId);
            if (selectedContatto == null)
            {
                MessageBox.Show("Contatto non trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var detailForm = _serviceProvider.GetRequiredService<ClientiContattiDetailForm>())
            {
                detailForm.SetContatto(selectedContatto);
                if (detailForm.ShowDialog() == DialogResult.OK)
                    await LoadContattiAsync();
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina Contatto".
        /// </summary>
        private async void btnEliminaContatto_Click(object? sender, EventArgs e)
        {
            int selectedId = GetSelectedRowId(dataGridViewContatti);
            if (selectedId == 0)
            {
                MessageBox.Show("Seleziona un contatto da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedContatto = await _clienteContattoRepository.GetByIdAsync(selectedId);
            if (selectedContatto == null)
            {
                MessageBox.Show("Contatto non trovato.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Sei sicuro di voler eliminare il contatto '{selectedContatto.Contatto}'?", "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    _clienteContattoRepository.Delete(selectedContatto);
                    await _clienteContattoRepository.SaveChangesAsync();
                    await LoadContattiAsync();
                    MessageBox.Show("Contatto eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Errore durante l'eliminazione del contatto: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

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
            lblRagSoc = new Label();
            txtRagSoc = new TextBox();
            btnSalva = new Button();
            lblCodiceFiscale = new Label();
            txtCodiceFiscale = new TextBox();
            groupBoxIndirizzi = new GroupBox();
            btnEliminaIndirizzo = new Button();
            btnModificaIndirizzo = new Button();
            btnNuovoIndirizzo = new Button();
            dataGridViewIndirizzi = new DataGridView();
            groupBoxContatti = new GroupBox();
            btnEliminaContatto = new Button();
            btnModificaContatto = new Button();
            btnNuovoContatto = new Button();
            dataGridViewContatti = new DataGridView();
            txtPIVA = new TextBox();
            label1 = new Label();
            txtIscrizAlbo = new TextBox();
            label2 = new Label();
            txtAutoCom = new TextBox();
            label3 = new Label();
            cmbTipo = new SearchableComboBox();
            label4 = new Label();
            btnAnnulla = new Button(); // Aggiunto: Se non lo avevi, assicurati di posizionarlo nel designer
            groupBoxIndirizzi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewIndirizzi).BeginInit();
            groupBoxContatti.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewContatti).BeginInit();
            SuspendLayout();
            // 
            // lblRagSoc
            // 
            lblRagSoc.AutoSize = true;
            lblRagSoc.Location = new Point(20, 30);
            lblRagSoc.Name = "lblRagSoc";
            lblRagSoc.Size = new Size(93, 15);
            lblRagSoc.TabIndex = 0;
            lblRagSoc.Text = "Ragione Sociale:";
            // 
            // txtRagSoc
            // 
            txtRagSoc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRagSoc.Location = new Point(120, 27);
            txtRagSoc.MaxLength = 50;
            txtRagSoc.Name = "txtRagSoc";
            txtRagSoc.Size = new Size(350, 23);
            txtRagSoc.TabIndex = 1;
            // 
            // btnSalva
            // 
            btnSalva.Location = new Point(685, 584);
            btnSalva.Name = "btnSalva";
            btnSalva.Size = new Size(75, 30);
            btnSalva.TabIndex = 2;
            btnSalva.Text = "Salva";
            btnSalva.UseVisualStyleBackColor = true;
            btnSalva.Click += btnSalva_Click;
            // 
            // lblCodiceFiscale
            // 
            lblCodiceFiscale.AutoSize = true;
            lblCodiceFiscale.Location = new Point(20, 79);
            lblCodiceFiscale.Name = "lblCodiceFiscale";
            lblCodiceFiscale.Size = new Size(85, 15);
            lblCodiceFiscale.TabIndex = 3;
            lblCodiceFiscale.Text = "Codice Fiscale:";
            // 
            // txtCodiceFiscale
            // 
            txtCodiceFiscale.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCodiceFiscale.Location = new Point(120, 76);
            txtCodiceFiscale.MaxLength = 16;
            txtCodiceFiscale.Name = "txtCodiceFiscale";
            txtCodiceFiscale.Size = new Size(193, 23);
            txtCodiceFiscale.TabIndex = 4;
            // 
            // groupBoxIndirizzi
            // 
            groupBoxIndirizzi.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxIndirizzi.Controls.Add(btnEliminaIndirizzo);
            groupBoxIndirizzi.Controls.Add(btnModificaIndirizzo);
            groupBoxIndirizzi.Controls.Add(btnNuovoIndirizzo);
            groupBoxIndirizzi.Controls.Add(dataGridViewIndirizzi);
            groupBoxIndirizzi.Location = new Point(12, 110);
            groupBoxIndirizzi.Name = "groupBoxIndirizzi";
            groupBoxIndirizzi.Size = new Size(760, 220);
            groupBoxIndirizzi.TabIndex = 5;
            groupBoxIndirizzi.TabStop = false;
            groupBoxIndirizzi.Text = "Indirizzi";
            // 
            // btnEliminaIndirizzo
            // 
            btnEliminaIndirizzo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEliminaIndirizzo.Location = new Point(174, 180);
            btnEliminaIndirizzo.Name = "btnEliminaIndirizzo";
            btnEliminaIndirizzo.Size = new Size(75, 30);
            btnEliminaIndirizzo.TabIndex = 3;
            btnEliminaIndirizzo.Text = "Elimina";
            btnEliminaIndirizzo.UseVisualStyleBackColor = true;
            // 
            // btnModificaIndirizzo
            // 
            btnModificaIndirizzo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnModificaIndirizzo.Location = new Point(93, 180);
            btnModificaIndirizzo.Name = "btnModificaIndirizzo";
            btnModificaIndirizzo.Size = new Size(75, 30);
            btnModificaIndirizzo.TabIndex = 2;
            btnModificaIndirizzo.Text = "Modifica";
            btnModificaIndirizzo.UseVisualStyleBackColor = true;
            // 
            // btnNuovoIndirizzo
            // 
            btnNuovoIndirizzo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNuovoIndirizzo.Location = new Point(12, 180);
            btnNuovoIndirizzo.Name = "btnNuovoIndirizzo";
            btnNuovoIndirizzo.Size = new Size(75, 30);
            btnNuovoIndirizzo.TabIndex = 1;
            btnNuovoIndirizzo.Text = "Nuovo";
            btnNuovoIndirizzo.UseVisualStyleBackColor = true;
            // 
            // dataGridViewIndirizzi
            // 
            dataGridViewIndirizzi.AllowUserToAddRows = false;
            dataGridViewIndirizzi.AllowUserToDeleteRows = false;
            dataGridViewIndirizzi.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewIndirizzi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewIndirizzi.Location = new Point(12, 22);
            dataGridViewIndirizzi.MultiSelect = false;
            dataGridViewIndirizzi.Name = "dataGridViewIndirizzi";
            dataGridViewIndirizzi.ReadOnly = true;
            dataGridViewIndirizzi.RowHeadersWidth = 82;
            dataGridViewIndirizzi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewIndirizzi.Size = new Size(736, 150);
            dataGridViewIndirizzi.TabIndex = 0;
            // 
            // groupBoxContatti
            // 
            groupBoxContatti.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBoxContatti.Controls.Add(btnEliminaContatto);
            groupBoxContatti.Controls.Add(btnModificaContatto);
            groupBoxContatti.Controls.Add(btnNuovoContatto);
            groupBoxContatti.Controls.Add(dataGridViewContatti);
            groupBoxContatti.Location = new Point(12, 340);
            groupBoxContatti.Name = "groupBoxContatti";
            groupBoxContatti.Size = new Size(760, 220);
            groupBoxContatti.TabIndex = 6;
            groupBoxContatti.TabStop = false;
            groupBoxContatti.Text = "Contatti";
            // 
            // btnEliminaContatto
            // 
            btnEliminaContatto.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnEliminaContatto.Location = new Point(174, 180);
            btnEliminaContatto.Name = "btnEliminaContatto";
            btnEliminaContatto.Size = new Size(75, 30);
            btnEliminaContatto.TabIndex = 3;
            btnEliminaContatto.Text = "Elimina";
            btnEliminaContatto.UseVisualStyleBackColor = true;
            // 
            // btnModificaContatto
            // 
            btnModificaContatto.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnModificaContatto.Location = new Point(93, 180);
            btnModificaContatto.Name = "btnModificaContatto";
            btnModificaContatto.Size = new Size(75, 30);
            btnModificaContatto.TabIndex = 2;
            btnModificaContatto.Text = "Modifica";
            btnModificaContatto.UseVisualStyleBackColor = true;
            // 
            // btnNuovoContatto
            // 
            btnNuovoContatto.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNuovoContatto.Location = new Point(12, 180);
            btnNuovoContatto.Name = "btnNuovoContatto";
            btnNuovoContatto.Size = new Size(75, 30);
            btnNuovoContatto.TabIndex = 1;
            btnNuovoContatto.Text = "Nuovo";
            btnNuovoContatto.UseVisualStyleBackColor = true;
            // 
            // dataGridViewContatti
            // 
            dataGridViewContatti.AllowUserToAddRows = false;
            dataGridViewContatti.AllowUserToDeleteRows = false;
            dataGridViewContatti.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewContatti.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewContatti.Location = new Point(12, 22);
            dataGridViewContatti.MultiSelect = false;
            dataGridViewContatti.Name = "dataGridViewContatti";
            dataGridViewContatti.ReadOnly = true;
            dataGridViewContatti.RowHeadersWidth = 82;
            dataGridViewContatti.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewContatti.Size = new Size(736, 150);
            dataGridViewContatti.TabIndex = 0;
            // 
            // txtPIVA
            // 
            txtPIVA.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPIVA.Location = new Point(120, 51);
            txtPIVA.MaxLength = 16;
            txtPIVA.Name = "txtPIVA";
            txtPIVA.Size = new Size(193, 23);
            txtPIVA.TabIndex = 8;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 54);
            label1.Name = "label1";
            label1.Size = new Size(40, 15);
            label1.TabIndex = 7;
            label1.Text = "P. IVA:";
            // 
            // txtIscrizAlbo
            // 
            txtIscrizAlbo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIscrizAlbo.Location = new Point(567, 30);
            txtIscrizAlbo.MaxLength = 50;
            txtIscrizAlbo.Name = "txtIscrizAlbo";
            txtIscrizAlbo.Size = new Size(193, 23);
            txtIscrizAlbo.TabIndex = 10;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(485, 33);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 9;
            label2.Text = "Iscriz. Albo:";
            // 
            // txtAutoCom
            // 
            txtAutoCom.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAutoCom.Location = new Point(567, 54);
            txtAutoCom.MaxLength = 50;
            txtAutoCom.Name = "txtAutoCom";
            txtAutoCom.Size = new Size(193, 23);
            txtAutoCom.TabIndex = 12;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(485, 57);
            label3.Name = "label3";
            label3.Size = new Size(75, 15);
            label3.TabIndex = 11;
            label3.Text = "Auto Comu.:";
            // 
            // cmbTipo
            // 
            cmbTipo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbTipo.Location = new Point(567, 79);
            cmbTipo.Name = "cmbTipo";
            cmbTipo.Size = new Size(193, 23);
            cmbTipo.TabIndex = 14;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(485, 82);
            label4.Name = "label4";
            label4.Size = new Size(34, 15);
            label4.TabIndex = 13;
            label4.Text = "Tipo:";
            // 
            // btnAnnulla
            // 
            btnAnnulla.Location = new Point(604, 584); // Esempio di posizione, adatta al tuo layout
            btnAnnulla.Name = "btnAnnulla";
            btnAnnulla.Size = new Size(75, 30);
            btnAnnulla.TabIndex = 15; // Un nuovo tabindex
            btnAnnulla.Text = "Annulla";
            btnAnnulla.UseVisualStyleBackColor = true;
            // 
            // ClientiDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 629);
            Controls.Add(btnAnnulla); // Aggiunto ai controlli del form
            Controls.Add(cmbTipo);
            Controls.Add(label4);
            Controls.Add(txtAutoCom);
            Controls.Add(label3);
            Controls.Add(txtIscrizAlbo);
            Controls.Add(label2);
            Controls.Add(txtPIVA);
            Controls.Add(label1);
            Controls.Add(groupBoxContatti);
            Controls.Add(groupBoxIndirizzi);
            Controls.Add(btnSalva);
            Controls.Add(txtCodiceFiscale);
            Controls.Add(lblCodiceFiscale);
            Controls.Add(txtRagSoc);
            Controls.Add(lblRagSoc);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ClientiDetailForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dettagli Cliente";
            groupBoxIndirizzi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewIndirizzi).EndInit();
            groupBoxContatti.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewContatti).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
    }
}