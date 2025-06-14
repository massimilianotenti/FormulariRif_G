// File: Forms/ClientiDetailForm.cs
// Questo form permette di visualizzare, creare e modificare i dettagli di un cliente.
// Ora funziona in modalità master-detail, mostrando e permettendo la gestione
// degli indirizzi e dei contatti del cliente direttamente da qui.
using FormulariRif_G.Data;
using FormulariRif_G.Models;
using Microsoft.Extensions.DependencyInjection; // Per IServiceProvider
using System.Linq;
using Microsoft.EntityFrameworkCore; // Per l'Include (anche se usiamo proiezione)

namespace FormulariRif_G.Forms
{
    public partial class ClientiDetailForm : Form
    {
        private readonly IGenericRepository<Cliente> _clienteRepository;
        private readonly IGenericRepository<ClienteIndirizzo> _clienteIndirizzoRepository;
        private readonly IGenericRepository<ClienteContatto> _clienteContattoRepository;
        private readonly IServiceProvider _serviceProvider;
        private Cliente? _currentCliente;
        private TextBox txtPIVA;
        private Label label1;
        private TextBox txtIscrizAlbo;
        private Label label2;
        private TextBox txtAutoCom;
        private Label label3;
        private TextBox txtTipo;
        private Label label4;
        private bool _isReadOnly;

        public ClientiDetailForm(IGenericRepository<Cliente> clienteRepository,
                                 IGenericRepository<ClienteIndirizzo> clienteIndirizzoRepository,
                                 IGenericRepository<ClienteContatto> clienteContattoRepository,
                                 IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _clienteRepository = clienteRepository;
            _clienteIndirizzoRepository = clienteIndirizzoRepository;
            _clienteContattoRepository = clienteContattoRepository;
            _serviceProvider = serviceProvider;
            this.Load += ClientiDetailForm_Load;
        }

        private async void ClientiDetailForm_Load(object? sender, EventArgs e)
        {
            if (_currentCliente?.Id != 0) // Se è un cliente esistente
            {
                await LoadIndirizziAsync();
                await LoadContattiAsync();
            }
            else
            {
                // Per un nuovo cliente, le griglie rimarranno vuote fino al salvataggio
                // e i pulsanti di gestione indirizzi/contatti saranno disabilitati.
            }
        }

        /// <summary>
        /// Imposta il cliente da visualizzare o modificare.
        /// </summary>
        /// <param name="cliente">L'oggetto Cliente.</param>
        /// <param name="isReadOnly">True per la modalità di sola lettura, False per la modifica.</param>
        public void SetCliente(Cliente cliente, bool isReadOnly = false)
        {
            _currentCliente = cliente;
            _isReadOnly = isReadOnly;
            LoadClienteData();
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
                txtTipo.Text = _currentCliente.Tipo ?? string.Empty;
            }
            else
            {
                // Inizializza i campi per un nuovo cliente
                txtRagSoc.Text = string.Empty;
                txtPIVA.Text = string.Empty;
                txtCodiceFiscale.Text = string.Empty;
                txtIscrizAlbo.Text = string.Empty;
                txtAutoCom.Text = string.Empty;
                txtTipo.Text = string.Empty;
            }
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
                    i.Indirizzo,
                    i.Comune,
                    i.Cap,
                    i.Predefinito
                }).ToList();

                dataGridViewIndirizzi.DataSource = displayIndirizzi;
                // Nascondi la colonna Id se non necessaria per la visualizzazione
                if (dataGridViewIndirizzi.Columns.Contains("Id"))
                {
                    dataGridViewIndirizzi.Columns["Id"].Visible = false;
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

                dataGridViewContatti.DataSource = displayContatti;
                // Nascondi la colonna Id se non necessaria per la visualizzazione
                if (dataGridViewContatti.Columns.Contains("Id"))
                {
                    dataGridViewContatti.Columns["Id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento dei contatti: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            btnSalva.Visible = !_isReadOnly;

            // I pulsanti di gestione indirizzi/contatti sono abilitati solo se il cliente è esistente e non in sola lettura
            bool enableSubCrud = !_isReadOnly && _currentCliente?.Id != 0;
            btnNuovoIndirizzo.Enabled = enableSubCrud;
            btnModificaIndirizzo.Enabled = enableSubCrud;
            btnEliminaIndirizzo.Enabled = enableSubCrud;
            btnNuovoContatto.Enabled = enableSubCrud;
            btnModificaContatto.Enabled = enableSubCrud;
            btnEliminaContatto.Enabled = enableSubCrud;
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Salva".
        /// </summary>
        private async void btnSalva_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            if (_currentCliente == null)
            {
                _currentCliente = new Cliente();
            }

            _currentCliente.RagSoc = txtRagSoc.Text.Trim();
            _currentCliente.PartitaIva = txtPIVA.Text.Trim();
            _currentCliente.CodiceFiscale = txtCodiceFiscale.Text.Trim();
            _currentCliente.Iscrizione_Albo = txtIscrizAlbo.Text.Trim();
            _currentCliente.Auto_Comunicazione = txtAutoCom.Text.Trim();
            _currentCliente.Tipo = txtTipo.Text.Trim();

            try
            {
                if (_currentCliente.Id == 0) // Nuovo cliente
                {
                    await _clienteRepository.AddAsync(_currentCliente);
                    await _clienteRepository.SaveChangesAsync(); // Salva per ottenere l'ID del nuovo cliente
                    MessageBox.Show("Cliente salvato con successo! Ora puoi aggiungere indirizzi e contatti.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Ricarica lo stato per abilitare i pulsanti di gestione
                    SetFormState();
                    // Non chiudere il form, l'utente potrebbe voler aggiungere indirizzi/contatti subito
                    this.DialogResult = DialogResult.OK; // Segnala che il cliente è stato salvato
                }
                else // Cliente esistente
                {
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
            return true;
        }

        #region Gestione Indirizzi

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo Indirizzo".
        /// </summary>
        private async void btnNuovoIndirizzo_Click(object sender, EventArgs e)
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
                {
                    // La logica di salvataggio e gestione del predefinito è nel detail form
                    await LoadIndirizziAsync(); // Ricarica la griglia dopo il salvataggio
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica Indirizzo".
        /// </summary>
        private async void btnModificaIndirizzo_Click(object sender, EventArgs e)
        {
            if (dataGridViewIndirizzi.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewIndirizzi.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedIndirizzo = await _clienteIndirizzoRepository.GetByIdAsync(selectedId);

                if (selectedIndirizzo != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<ClientiIndirizzoDetailForm>())
                    {
                        detailForm.SetIndirizzo(selectedIndirizzo);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            // La logica di salvataggio e gestione del predefinito è nel detail form
                            await LoadIndirizziAsync(); // Ricarica la griglia dopo il salvataggio
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un indirizzo da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina Indirizzo".
        /// </summary>
        private async void btnEliminaIndirizzo_Click(object sender, EventArgs e)
        {
            if (dataGridViewIndirizzi.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewIndirizzi.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedIndirizzo = await _clienteIndirizzoRepository.GetByIdAsync(selectedId);

                if (selectedIndirizzo != null)
                {
                    // Logica per prevenire l'eliminazione dell'unico indirizzo predefinito
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
                                await LoadIndirizziAsync(); // Ricarica per riflettere il nuovo predefinito
                            }

                            MessageBox.Show("Indirizzo eliminato con successo.", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Errore durante l'eliminazione dell'indirizzo: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un indirizzo da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Gestione Contatti

        /// <summary>
        /// Gestisce il click sul pulsante "Nuovo Contatto".
        /// </summary>
        private async void btnNuovoContatto_Click(object sender, EventArgs e)
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
                {
                    await LoadContattiAsync(); // Ricarica la griglia dopo il salvataggio
                }
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Modifica Contatto".
        /// </summary>
        private async void btnModificaContatto_Click(object sender, EventArgs e)
        {
            if (dataGridViewContatti.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewContatti.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedContatto = await _clienteContattoRepository.GetByIdAsync(selectedId);

                if (selectedContatto != null)
                {
                    using (var detailForm = _serviceProvider.GetRequiredService<ClientiContattiDetailForm>())
                    {
                        detailForm.SetContatto(selectedContatto);
                        if (detailForm.ShowDialog() == DialogResult.OK)
                        {
                            await LoadContattiAsync(); // Ricarica la griglia dopo il salvataggio
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleziona un contatto da modificare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Gestisce il click sul pulsante "Elimina Contatto".
        /// </summary>
        private async void btnEliminaContatto_Click(object sender, EventArgs e)
        {
            if (dataGridViewContatti.SelectedRows.Count > 0)
            {
                // Recupera l'ID dal tipo anonimo e poi l'oggetto completo dal repository
                var selectedRowData = dataGridViewContatti.SelectedRows[0].DataBoundItem;
                int selectedId = (int)selectedRowData.GetType().GetProperty("Id").GetValue(selectedRowData);
                var selectedContatto = await _clienteContattoRepository.GetByIdAsync(selectedId);

                if (selectedContatto != null)
                {
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
            }
            else
            {
                MessageBox.Show("Seleziona un contatto da eliminare.", "Avviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            txtTipo = new TextBox();
            label4 = new Label();
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
            btnEliminaIndirizzo.Click += btnEliminaIndirizzo_Click;
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
            btnModificaIndirizzo.Click += btnModificaIndirizzo_Click;
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
            btnNuovoIndirizzo.Click += btnNuovoIndirizzo_Click;
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
            btnEliminaContatto.Click += btnEliminaContatto_Click;
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
            btnModificaContatto.Click += btnModificaContatto_Click;
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
            btnNuovoContatto.Click += btnNuovoContatto_Click;
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
            // txtTipo
            // 
            txtTipo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTipo.Location = new Point(567, 79);
            txtTipo.MaxLength = 50;
            txtTipo.Name = "txtTipo";
            txtTipo.Size = new Size(193, 23);
            txtTipo.TabIndex = 14;
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
            // ClientiDetailForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 629);
            Controls.Add(txtTipo);
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

        #endregion
    }
}
